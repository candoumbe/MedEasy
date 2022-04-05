namespace Identity.API.Features.v1.Auth
{
    using Identity.API.Features.Auth;
    using Identity.CQRS.Commands;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DTO;
    using Identity.DTO.Auth;
    using Identity.DTO.v1;
    using Identity.ValueObjects;

    using MedEasy.CQRS.Core.Commands.Results;

    using MediatR;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    using Optional;

    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using static Microsoft.AspNetCore.Http.StatusCodes;

    /// <summary>
    /// Endpoint to handle authentication, token.
    /// </summary>
    [ApiController]
    [Route("/auth/[controller]")]
    [Authorize]
    public class TokenController
    {
        private readonly IMediator _mediator;
        private readonly IOptionsSnapshot<JwtOptions> _jwtOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Builds a new <see cref="TokenController"/> instance.
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="jwtOptions"></param>
        /// <param name="httpContextAccessor"></param>
        public TokenController(IMediator mediator,
                               IOptionsSnapshot<JwtOptions> jwtOptions,
                               IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _jwtOptions = jwtOptions;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Generates a token for the specified user
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ct">Notifies to abort the action execution</param>
        /// <returns></returns>
        /// <response code="404">The login/password was not found</response>
        [ApiVersion("1.0")]
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status200OK, Type = typeof(BearerTokenInfo))]
        public async ValueTask<IActionResult> Post([FromBody, BindRequired] LoginModel model, CancellationToken ct = default)
        {
            LoginInfo loginInfo = new() { Username = UserName.From(model.Username), Password = model.Password };
            Option<AccountInfo> optionalUser = await _mediator.Send(new GetOneAccountByUsernameAndPasswordQuery(loginInfo), ct)
                .ConfigureAwait(false);

            return await optionalUser.Match<ValueTask<IActionResult>>(
                some: async accountInfo =>
                {
                    JwtOptions jwtOptions = _jwtOptions.Value;
                    _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X_FORWARDED_FOR", out StringValues ipValues);
                    AuthenticationInfo authenticationInfo = new() { Location = ipValues.ToArray().FirstOrDefault() ?? string.Empty };
                    JwtInfos jwtInfos = new()
                    {
                        Key = jwtOptions.Key,
                        Issuer = jwtOptions.Issuer,
                        Audiences = jwtOptions.Audiences,
                        AccessTokenLifetime = jwtOptions.AccessTokenLifetime,
                        RefreshTokenLifetime = jwtOptions.RefreshTokenLifetime
                    };
                    AuthenticationTokenInfo token = await _mediator.Send(new CreateAuthenticationTokenCommand((authenticationInfo, accountInfo, jwtInfos)), ct)
                        .ConfigureAwait(false);

                    string accessTokenString;
                    string refreshTokenString;
                    JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
                    switch (token.AccessToken)
                    {
                        case JwtSecurityToken jwtToken:
                            accessTokenString = jwtSecurityTokenHandler.WriteToken(jwtToken);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Unhandled access token type");
                    }

                    switch (token.RefreshToken)
                    {
                        case JwtSecurityToken jwtToken:
                            refreshTokenString = jwtSecurityTokenHandler.WriteToken(jwtToken);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Unhandled refresh token type");
                    }

                    return new OkObjectResult(new BearerTokenInfo
                    {
                        AccessToken = accessTokenString,
                        RefreshToken = refreshTokenString
                    });
                },
                none: () => new ValueTask<IActionResult>(new NotFoundResult())
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Invalidates a refresh token previously obtained after a successfull login.
        /// </summary>
        /// <param name="username">Username of the account to invalidate.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <reponse code="204">The token was successfully invalidated</reponse>
        /// <reponse code="401">The token sent alongside the request has expired</reponse>
        /// <reponse code="409">The request tries to invalidate </reponse>
        [HttpDelete("{username}")]
        [ApiVersion("1.0")]
        [ApiVersion("2.0")]
        [ProducesResponseType(Status204NoContent)]
        public async Task<IActionResult> Invalidate(string username, CancellationToken ct = default)
        {
            InvalidateAccessTokenByUsernameCommand cmd = new(UserName.From(username));
            InvalidateAccessCommandResult cmdResult = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            IActionResult actionResult;
            switch (cmdResult)
            {
                case InvalidateAccessCommandResult.Done:
                    actionResult = new NoContentResult();
                    break;
                case InvalidateAccessCommandResult.Failed_Unauthorized:
                    actionResult = new UnauthorizedResult();
                    break;
                case InvalidateAccessCommandResult.Failed_NotFound:
                    actionResult = new NotFoundResult();
                    break;
                case InvalidateAccessCommandResult.Failed_Conflict:
                    actionResult = new StatusCodeResult(Status409Conflict);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown <{cmdResult}> command result");
            }

            return actionResult;
        }

        /// <summary>
        /// Renews access token
        /// </summary>
        /// <param name="username">Username of the account to renew access token for</param>
        /// <param name="refreshAccessToken">Access token and refresh token to renew</param>
        /// <param name="ct"></param>
        /// <response code="200">Un nouveau jeton d'accès a été forgé</response>
        /// <response code="404">l'utilisateur n'existe pas.</response>
        /// <response code="404">l'utilisateur n'existe pas.</response>
        [HttpPut("{username}")]
        [ApiVersion("1.0")]
        [ApiVersion("2.0")]
        [ProducesResponseType(typeof(BearerTokenInfo), Status200OK)]

        public async Task<IActionResult> Refresh(string username, [FromBody] RefreshAccessTokenInfo refreshAccessToken, CancellationToken ct = default)
        {
            JwtOptions jwtOptions = _jwtOptions.Value;
            JwtInfos jwtInfos = new()
            {
                AccessTokenLifetime = jwtOptions.AccessTokenLifetime,
                Audiences = jwtOptions.Audiences,
                Issuer = jwtOptions.Issuer,
                Key = jwtOptions.Key,
                RefreshTokenLifetime = jwtOptions.RefreshTokenLifetime
            };
            RefreshAccessTokenByUsernameCommand request = new((UserName.From(username), refreshAccessToken.AccessToken, refreshAccessToken.RefreshToken, jwtInfos));
            Option<BearerTokenInfo, RefreshAccessCommandResult> optionalBearerToken = await _mediator.Send(request, ct)
                .ConfigureAwait(false);

            return optionalBearerToken.Match<IActionResult>(
                some: bearerToken => new OkObjectResult(bearerToken),
                none: cmdResult =>
                {
                    return cmdResult switch
                    {
                        RefreshAccessCommandResult.NotFound => new NotFoundResult(),
                        RefreshAccessCommandResult.Conflict => new StatusCodeResult(Status409Conflict),
                        RefreshAccessCommandResult.Unauthorized => new UnauthorizedResult(),
                        _ => throw new ArgumentOutOfRangeException(nameof(cmdResult)),
                    };
                });
        }
    }
}
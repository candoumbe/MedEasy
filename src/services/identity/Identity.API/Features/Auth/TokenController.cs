using Identity.CQRS.Commands;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using Identity.DTO.Auth;
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
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Identity.API.Features.Auth
{
    [ApiController]
    [Route("auth/[controller]")]
    [Authorize]
    public class TokenController
    {
        private readonly IMediator _mediator;
        private readonly IOptionsSnapshot<JwtOptions> _jwtOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenController(IMediator mediator, IOptionsSnapshot<JwtOptions> jwtOptions, IHttpContextAccessor httpContextAccessor)
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
        [HttpPost]
        [AllowAnonymous]
        public async ValueTask<IActionResult> Post([FromBody, BindRequired]LoginModel model, CancellationToken ct = default)
        {
            LoginInfo loginInfo = new LoginInfo { Username = model.Username, Password = model.Password };
            Option<AccountInfo> optionalUser = await _mediator.Send(new GetOneAccountByUsernameAndPasswordQuery(loginInfo), ct)
                .ConfigureAwait(false);

            return await optionalUser.Match<ValueTask<IActionResult>>(
                some: async accountInfo =>
                {
                    JwtOptions jwtOptions = _jwtOptions.Value;
                    _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X_FORWARDED_FOR", out StringValues ipValues);
                    AuthenticationInfo authenticationInfo = new AuthenticationInfo { Location = ipValues.ToArray().FirstOrDefault() ?? string.Empty };
                    JwtInfos jwtInfos = new JwtInfos
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
                    switch (token.AccessToken)
                    {
                        case JwtSecurityToken jwtToken:
                            accessTokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Unhandled access token type");
                    }

                    switch (token.RefreshToken)
                    {
                        case JwtSecurityToken jwtToken:
                            refreshTokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Unhandled refresh token type");
                    }

                    return new OkObjectResult( new BearerTokenInfo
                    {
                        AccessToken = accessTokenString,
                        RefreshToken = refreshTokenString
                    });
                },
                none: () => new ValueTask<IActionResult>(new BadRequestResult())
            ).ConfigureAwait(false);
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> Invalidate(string username, CancellationToken ct = default)
        {
            InvalidateAccessTokenByUsernameCommand cmd = new InvalidateAccessTokenByUsernameCommand(username);
            InvalidateAccessCommandResult cmdResult = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            IActionResult actionResult;
            switch (cmdResult)
            {
                case InvalidateAccessCommandResult.Done:
                    actionResult = new OkResult();
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

        [HttpPatch("{username}")]
        [Consumes("application/json", "application/xml")]
        public async Task<IActionResult> Refresh(string username, [FromBody, BindRequired] RefreshAccessTokenInfo refreshAccessToken, CancellationToken ct = default)
        {
            JwtOptions jwtOptions = _jwtOptions.Value;
            JwtInfos jwtInfos = new JwtInfos
            {
                AccessTokenLifetime = jwtOptions.AccessTokenLifetime,
                Audiences = jwtOptions.Audiences,
                Issuer = jwtOptions.Issuer,
                Key = jwtOptions.Key,
                RefreshTokenLifetime = jwtOptions.RefreshTokenLifetime
            };
            RefreshAccessTokenByUsernameCommand request = new RefreshAccessTokenByUsernameCommand((username, refreshAccessToken.AccessToken, refreshAccessToken.RefreshToken, jwtInfos));
            Option<BearerTokenInfo, RefreshAccessCommandResult> optionalBearerToken = await _mediator.Send(request, ct)
                .ConfigureAwait(false);

            return optionalBearerToken.Match(
                some: bearerToken => new OkObjectResult(bearerToken),
                none: cmdResult =>
                {
                    IActionResult actionResult;
                    switch (cmdResult)
                    {
                        case RefreshAccessCommandResult.NotFound:
                            actionResult = new NotFoundResult();
                            break;
                        case RefreshAccessCommandResult.Conflict:
                            actionResult = new StatusCodeResult(Status409Conflict);
                            break;
                        case RefreshAccessCommandResult.Unauthorized:
                            actionResult = new UnauthorizedResult();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return actionResult;
                });
        }
    }
}
using Identity.API.Features.Auth;
using Identity.API.Features.v1.Auth;
using Identity.CQRS.Commands;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using Identity.DTO.v2;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

using Optional;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Identity.API.Features.v2.Auth
{
    /// <summary>
    /// Endpoint to handle authentication, token.
    /// </summary>
    [ApiController]
    [ApiVersion("2.0")]
    [Route("v{version:apiVersion}/auth/[controller]")]
    [Authorize]
    public class TokenController
    {
        private readonly IMediator _mediator;
        private readonly IOptionsMonitor<JwtOptions> _jwtOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Endpoint to create/delete tokens
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="jwtOptions"></param>
        /// <param name="httpContextAccessor"></param>
        public TokenController(IMediator mediator, IOptionsMonitor<JwtOptions> jwtOptions, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _jwtOptions = jwtOptions;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Generates a token for the specified user
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ipValues"></param>
        /// <param name="ct">Notifies to abort the action execution</param>
        /// <returns></returns>
        /// <response code="404">The login/password was not found</response>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status200OK, Type = typeof(BearerTokenInfo))]
        [ApiVersion("2.0")]
        public async ValueTask<ActionResult<BearerTokenInfo>> Post([FromBody, BindRequired] LoginModel model, [FromHeader(Name = "x-forwarder-for")] IEnumerable<string> ipValues = default, CancellationToken ct = default)
        {
            LoginInfo loginInfo = new() { Username = model.Username, Password = model.Password };
            Option<AccountInfo> optionalUser = await _mediator.Send(new GetOneAccountByUsernameAndPasswordQuery(loginInfo), ct)
                .ConfigureAwait(false);

            return await optionalUser.Match<ValueTask<ActionResult<BearerTokenInfo>>>(
                some: async accountInfo =>
                {
                    JwtOptions jwtOptions = _jwtOptions.CurrentValue;
                    //_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X_FORWARDED_FOR", out StringValues ipValues);
                    AuthenticationInfo authenticationInfo = new() { Location = ipValues?.ToArray()?.FirstOrDefault() ?? string.Empty };
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

                    (string Token, DateTime Expires) accessToken;
                    (string Token, DateTime Expires) refreshToken;
                    JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
                    switch (token.AccessToken)
                    {
                        case JwtSecurityToken jwtToken:
                            accessToken = (jwtSecurityTokenHandler.WriteToken(jwtToken), jwtToken.ValidTo);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Unhandled access token type");
                    }

                    switch (token.RefreshToken)
                    {
                        case JwtSecurityToken jwtToken:
                            refreshToken = (jwtSecurityTokenHandler.WriteToken(jwtToken), jwtToken.ValidTo);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Unhandled refresh token type");
                    }

                    return new BearerTokenInfo
                    {
                        AccessToken = new TokenInfo
                        {
                            Token = accessToken.Token,
                            Expires = accessToken.Expires
                        },
                        RefreshToken = new TokenInfo
                        {
                            Token = refreshToken.Token,
                            Expires = refreshToken.Expires
                        }
                    };
                },
                none: () => new ValueTask<ActionResult<BearerTokenInfo>>(new NotFoundResult())
            ).ConfigureAwait(false);
        }
    }
}
using Identity.CQRS.Commands;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using MedEasy.Identity.API.Features.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Optional;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Features.Authentication
{
    [Controller]
    [Route("auth/[controller]")]
    public class TokenController
    {
        private readonly IMediator _mediator;
        private readonly IOptionsSnapshot<JwtOptions> _jwtOptions;

        public TokenController(IMediator mediator, IOptionsSnapshot<JwtOptions> jwtOptions)
        {
            _mediator = mediator;
            _jwtOptions = jwtOptions;
        }

        /// <summary>
        /// Generates a token for the specified user
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ct">Notifies to abort the action execution</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async ValueTask<IActionResult> Post([FromBody]LoginModel model, CancellationToken ct = default)
        {
            LoginInfo loginInfo = new LoginInfo { Username = model.Username, Password = model.Password };
            Option<AccountInfo> optionalUser = await _mediator.Send(new GetOneAccountByUsernameAndPasswordQuery(loginInfo), ct)
                .ConfigureAwait(false);

            return await optionalUser.Match<ValueTask<IActionResult>>(
                some: async accountInfo =>
                {
                    JwtOptions jwtOptions = _jwtOptions.Value;
                    AuthenticationInfo authenticationInfo = new AuthenticationInfo { Location = "Paris - France" };
                    JwtInfos jwtInfos = new JwtInfos
                    {
                        Key = jwtOptions.Key,
                        Issuer = jwtOptions.Issuer,
                        Audiences = jwtOptions.Audiences,
                        AccessTokenValidity = jwtOptions.Validity,
                        RefreshTokenValidity = jwtOptions.Validity * 2

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

                    BearerTokenInfo bearerToken = new BearerTokenInfo
                    {
                        AccessToken = accessTokenString,
                        RefreshToken = refreshTokenString
                    };

                    return new OkObjectResult(bearerToken);
                },
                none: () => new ValueTask<IActionResult>(new UnauthorizedResult())
            );
        }
    }
}
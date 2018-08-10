using Identity.CQRS.Commands;
using Identity.DTO;
using MedEasy.Abstractions;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Commands
{
    /// <summary>
    /// Handles creation of token suitable for authenticating an <see cref="AccountInfo"/>.
    /// </summary>
    public class HandleCreateAuthenticationTokenCommand : IRequestHandler<CreateAuthenticationTokenCommand, SecurityToken>
    {
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Builds a new <see cref="HandleCreateAuthenticationTokenCommand"/> instance.
        /// </summary>
        /// <param name="dateTimeService">Service that provide methods to get current date.</param>
        public HandleCreateAuthenticationTokenCommand(IDateTimeService dateTimeService) => _dateTimeService = dateTimeService;

        public Task<SecurityToken> Handle(CreateAuthenticationTokenCommand cmd, CancellationToken ct)
        {
            DateTime now = _dateTimeService.UtcNow();
            (AccountInfo accountInfo, JwtInfos jwtInfos) = cmd.Data;


            IEnumerable<string> audiences = jwtInfos.Audiences.Any()
                ? jwtInfos.Audiences.Skip(1)
                : Enumerable.Empty<string>();

            IEnumerable<Claim> claims = new[]{
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(CustomClaimTypes.AccountId, accountInfo.Id.ToString()),
                    new Claim(ClaimTypes.Name, accountInfo.Name ?? accountInfo.Username),
                    new Claim(ClaimTypes.NameIdentifier, accountInfo.Username),
                    new Claim(ClaimTypes.Email, accountInfo.Email),
                    new Claim(ClaimTypes.GivenName, accountInfo.Name ?? accountInfo.Username)
                }
            .Union(
                    accountInfo.Claims.Select(claim => new Claim(claim.Type, claim.Value))
            .Union(audiences.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)))
);

            SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtInfos.Key));
            SecurityToken token = new JwtSecurityToken(
                jwtInfos.Issuer,
                jwtInfos.Audiences.Any() ? jwtInfos.Audiences.First() : jwtInfos.Issuer,
                claims,
                notBefore: now,
                expires: now.AddMinutes(jwtInfos.Validity),
                new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );



            return new ValueTask<SecurityToken>(token).AsTask();



        }
    }
}

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
            DateTimeOffset now = _dateTimeService.UtcNowOffset();
            (AccountInfo accountInfo, JwtInfos jwtInfos) = cmd.Data;
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtInfos.Key));
            IEnumerable<string> audiences = jwtInfos.Audiences.Any()
                ? jwtInfos.Audiences.Skip(1)
                : Enumerable.Empty<string>();
            IEnumerable<Claim> claims = accountInfo.Claims
                    .Select(claim => new Claim(claim.Type, claim.Value))
                    .Union(audiences.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)))
                    .Union(new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    });
            SecurityToken token = new JwtSecurityToken(
                jwtInfos.Issuer,
                jwtInfos.Audiences.Any() ? jwtInfos.Audiences.First() : jwtInfos.Issuer,
                claims,
                now.DateTime,
                now.AddMinutes(jwtInfos.Validity).DateTime,
                new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );



            return Task.FromResult(token);



        }
    }
}

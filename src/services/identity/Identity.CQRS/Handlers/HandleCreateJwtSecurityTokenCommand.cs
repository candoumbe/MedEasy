using Identity.CQRS.Commands;
using Identity.DTO;

using MedEasy.Abstractions;

using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers
{
    /// <summary>
    /// Handles creation of token suitable for authenticating an <see cref="AccountInfo"/>.
    /// </summary>
    public class HandleCreateJwtSecurityTokenCommand : IHandleCreateSecurityTokenCommand
    {
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Builds a new <see cref="HandleCreateAuthenticationTokenCommand"/> instance.
        /// </summary>
        /// <param name="dateTimeService">Service that provide methods to get current date.</param>
        public HandleCreateJwtSecurityTokenCommand(IDateTimeService dateTimeService) => _dateTimeService = dateTimeService;

        public Task<SecurityToken> Handle(CreateSecurityTokenCommand cmd, CancellationToken ct)
        {
            DateTime now = _dateTimeService.UtcNow();
            (JwtSecurityTokenOptions tokenOptions, IEnumerable<ClaimInfo> claims) data = cmd.Data;

            IEnumerable<string> audiences = data.tokenOptions.Audiences?.Distinct() ?? Enumerable.Empty<string>();

            IEnumerable<ClaimInfo> claims = data.claims ?? Enumerable.Empty<ClaimInfo>();
            if (!claims.Any(claim => claim.Type == JwtRegisteredClaimNames.Sid))
            {
                claims = claims.Concat(new[] { new ClaimInfo { Type = JwtRegisteredClaimNames.Jti, Value = Guid.NewGuid().ToString() } });
            }


            SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(data.tokenOptions.Key));
            SecurityToken token = new JwtSecurityToken(
                issuer: data.tokenOptions.Issuer,
                audience: audiences.Any()
                    ? data.tokenOptions.Audiences.First()
                    : data.tokenOptions.Issuer,
                claims: claims.Select(claim => new Claim(claim.Type, claim.Value))
                    .Concat(audiences.Skip(1).Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience))),
                notBefore: now,
                expires: now.AddMinutes(data.tokenOptions.LifetimeInMinutes),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            return new ValueTask<SecurityToken>(token).AsTask();
        }
    }
}

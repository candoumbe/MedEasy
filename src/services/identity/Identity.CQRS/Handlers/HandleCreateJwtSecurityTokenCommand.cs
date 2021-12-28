namespace Identity.CQRS.Handlers
{
    using Identity.CQRS.Commands;
    using Identity.DTO;

    using Microsoft.IdentityModel.Tokens;

    using NodaTime;

    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles creation of token suitable for authenticating an <see cref="AccountInfo"/>.
    /// </summary>
    public class HandleCreateJwtSecurityTokenCommand : IHandleCreateSecurityTokenCommand
    {
        private readonly IClock _dateTimeService;

        /// <summary>
        /// Builds a new <see cref="HandleCreateAuthenticationTokenCommand"/> instance.
        /// </summary>
        /// <param name="dateTimeService">Service that provide methods to get current date.</param>
        public HandleCreateJwtSecurityTokenCommand(IClock dateTimeService) => _dateTimeService = dateTimeService;

        public Task<SecurityToken> Handle(CreateSecurityTokenCommand cmd, CancellationToken ct)
        {
            (JwtSecurityTokenOptions tokenOptions, Instant now, IEnumerable<ClaimInfo> claims) data = cmd.Data;

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
                notBefore: data.now.ToDateTimeUtc(),
                expires: data.now.Plus(Duration.FromMinutes(data.tokenOptions.LifetimeInMinutes))
                            .ToDateTimeUtc(),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            return new ValueTask<SecurityToken>(token).AsTask();
        }
    }
}

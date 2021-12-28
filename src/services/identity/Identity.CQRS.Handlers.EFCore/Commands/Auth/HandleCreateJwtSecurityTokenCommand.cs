namespace Identity.CQRS.Handlers.EFCore.Commands.Auth
{
    using Identity.CQRS.Commands;
    using Identity.DTO;

    using Microsoft.Extensions.Logging;
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
        private readonly ILogger<HandleCreateJwtSecurityTokenCommand> _logger;

        /// <summary>
        /// Builds a new <see cref="HandleCreateAuthenticationTokenCommand"/> instance.
        /// </summary>
        /// <param name="logger">Logger</param>
        public HandleCreateJwtSecurityTokenCommand(ILogger<HandleCreateJwtSecurityTokenCommand> logger)
        {
            _logger = logger;
        }

        ///<inheritdoc/>
        public Task<SecurityToken> Handle(CreateSecurityTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Start handling command {CommandId}", request.Id);
            (JwtSecurityTokenOptions tokenOptions, Instant from, IEnumerable<ClaimInfo> claims) data = request.Data;

            IEnumerable<string> audiences = data.tokenOptions.Audiences?.Distinct() ?? Enumerable.Empty<string>();

            IEnumerable<ClaimInfo> claims = data.claims ?? Enumerable.Empty<ClaimInfo>();
            if (!claims.Any(claim => claim.Type == JwtRegisteredClaimNames.Sid))
            {
                claims = claims.Concat(new[] { new ClaimInfo { Type = JwtRegisteredClaimNames.Jti, Value = Guid.NewGuid().ToString() } });
            }

            SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(data.tokenOptions.Key));
            Instant expires = data.from.Plus(Duration.FromMinutes(data.tokenOptions.LifetimeInMinutes));
            SecurityToken token = new JwtSecurityToken(
                issuer: data.tokenOptions.Issuer,
                audience: audiences.Any()
                    ? data.tokenOptions.Audiences.First()
                    : data.tokenOptions.Issuer,
                claims: claims.Select(claim => new Claim(claim.Type, claim.Value))
                              .Concat(audiences.Skip(1).Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience))),
                notBefore: data.from.ToDateTimeUtc(),
                expires: expires.ToDateTimeUtc(),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );
            _logger.LogDebug("Token will be valid from {Start} to {End}", data.from, expires);

            _logger.LogDebug("Finished handling command {CommandId}", request.Id);
            return new ValueTask<SecurityToken>(token).AsTask();
        }
    }
}

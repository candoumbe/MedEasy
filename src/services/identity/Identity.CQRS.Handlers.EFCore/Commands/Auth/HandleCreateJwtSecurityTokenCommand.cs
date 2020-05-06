using Identity.CQRS.Commands;
using Identity.DTO;
using MedEasy.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.EFCore.Commands.Auth
{
    /// <summary>
    /// Handles creation of token suitable for authenticating an <see cref="AccountInfo"/>.
    /// </summary>
    public class HandleCreateJwtSecurityTokenCommand : IHandleCreateSecurityTokenCommand
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<HandleCreateJwtSecurityTokenCommand> _logger;

        /// <summary>
        /// Builds a new <see cref="HandleCreateAuthenticationTokenCommand"/> instance.
        /// </summary>
        /// <param name="dateTimeService">Service that provide methods to get current date.</param>
        /// <param name="logger">Logger</param>
        public HandleCreateJwtSecurityTokenCommand(IDateTimeService dateTimeService, ILogger<HandleCreateJwtSecurityTokenCommand> logger)
        {
            _dateTimeService = dateTimeService;
            _logger = logger;
        }

        public Task<SecurityToken> Handle(CreateSecurityTokenCommand cmd, CancellationToken ct)
        {
            _logger.LogDebug("Start handling command {CommandId}", cmd.Id);
            DateTime now = _dateTimeService.UtcNow();
            (JwtSecurityTokenOptions tokenOptions, IEnumerable<ClaimInfo> claims) data = cmd.Data;

            IEnumerable<string> audiences = data.tokenOptions.Audiences?.Distinct() ?? Enumerable.Empty<string>();

            IEnumerable<ClaimInfo> claims = (data.claims ?? Enumerable.Empty<ClaimInfo>());
            if (!claims.Any(claim => claim.Type == JwtRegisteredClaimNames.Sid))
            {
                claims = claims.Concat(new[] { new ClaimInfo { Type = JwtRegisteredClaimNames.Jti, Value = Guid.NewGuid().ToString() } });
            }

            SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(data.tokenOptions.Key));
            DateTime expires = now.AddMinutes(data.tokenOptions.LifetimeInMinutes);
            SecurityToken token = new JwtSecurityToken(
                issuer : data.tokenOptions.Issuer,
                audience : audiences.Any()
                    ? data.tokenOptions.Audiences.First()
                    : data.tokenOptions.Issuer,
                claims : claims.Select(claim => new Claim(claim.Type, claim.Value))
                    .Concat(audiences.Skip(1).Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience))),
                notBefore: now,
                expires: expires,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );
            _logger.LogDebug("Token will be valid from {Start} to {End}", now, expires);

            _logger.LogDebug("Finished handling command {CommandId}", cmd.Id);
            return new ValueTask<SecurityToken>(token).AsTask();
        }
    }
}

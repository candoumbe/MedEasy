namespace Identity.CQRS.Commands
{
    using Identity.DTO;

    using MedEasy.CQRS.Core.Commands;

    using Microsoft.IdentityModel.Tokens;

    using NodaTime;

    using System;
    using System.Collections.Generic;

    public class CreateSecurityTokenCommand : CommandBase<Guid, (JwtSecurityTokenOptions tokenOptions, Instant validFrom, IEnumerable<ClaimInfo> claims), SecurityToken>
    {
        /// <summary>
        /// Builds a new <see cref="CreateRefreshTokenCommand"/> instance.
        /// </summary>
        /// <param name="data"><see cref="AccountInfo"/> to create one token for.</param>
        public CreateSecurityTokenCommand((JwtSecurityTokenOptions tokenOptions, Instant validFrom, IEnumerable<ClaimInfo> claims) data) : base(Guid.NewGuid(), data)
        {

        }
    }
}

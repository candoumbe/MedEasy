using Identity.DTO;
using MedEasy.CQRS.Core.Commands;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Identity.CQRS.Commands
{
    /// <summary>
    /// Requests creation of a token which can be later used to authenticate an <see cref="AccountInfo"/>.
    /// </summary>
    public class CreateAuthenticationTokenCommand : CommandBase<Guid, (AccountInfo accountInfo, JwtInfos jwtInfos), SecurityToken>
    {
        /// <summary>
        /// Builds a new <see cref="CreateAuthenticationTokenCommand"/> instance.
        /// </summary>
        /// <param name="data"><see cref="AccountInfo"/> to create one token for.</param>
        public CreateAuthenticationTokenCommand((AccountInfo accountInfo, JwtInfos jwtInfos) data) : base(Guid.NewGuid(), data)
        {
        }
    }
}

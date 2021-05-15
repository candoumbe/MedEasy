namespace Identity.CQRS.Commands
{
    using Identity.DTO;

    using MedEasy.CQRS.Core.Commands;

    using System;

    /// <summary>
    /// Requests creation of a token which can be later used to authenticate an <see cref="AccountInfo"/>.
    /// </summary>
    public class CreateAuthenticationTokenCommand : CommandBase<Guid, (AuthenticationInfo authInfo, AccountInfo accountInfo, JwtInfos jwtInfos), AuthenticationTokenInfo>
    {
        /// <summary>
        /// Builds a new <see cref="CreateAuthenticationTokenCommand"/> instance.
        /// </summary>
        /// <param name="data"><see cref="AccountInfo"/> to create one token for.</param>
        public CreateAuthenticationTokenCommand((AuthenticationInfo authInfo, AccountInfo accountInfo, JwtInfos jwtInfos) data) : base(Guid.NewGuid(), data)
        {
        }
    }
}

using Identity.DTO;

using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;

using Optional;

using System;

namespace Identity.CQRS.Commands.Accounts
{
    /// <summary>
    /// Command to create a new <see cref="AccountInfo"/>.
    /// </summary>
    public class CreateAccountInfoCommand : CommandBase<Guid, NewAccountInfo, Option<AccountInfo, CreateCommandResult>>
    {
        /// <summary>
        /// Builds a new <see cref="CreateAccountInfoCommand"/> instance.
        /// </summary>
        /// <param name="data">data used to create the <see cref="AccountInfo"/></param>
        public CreateAccountInfoCommand(NewAccountInfo data) : base(Guid.NewGuid(), data)
        {

        }
    }
}

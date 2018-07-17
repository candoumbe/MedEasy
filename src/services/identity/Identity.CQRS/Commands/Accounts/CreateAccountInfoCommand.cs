using Identity.DTO;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.CQRS.Commands.Accounts
{
    /// <summary>
    /// Command to create a new <see cref="AccountInfo"/>.
    /// </summary>
    public class CreateAccountInfoCommand : CommandBase<Guid, NewAccountInfo, Option<AccountInfo,CreateCommandResult>>
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

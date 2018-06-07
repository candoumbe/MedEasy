using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Identity.CQRS.Commands.Accounts
{
    /// <summary>
    /// Command to delete an <see cref="Accountinfo"/> by its <see cref="AccountInfo.Id"/>
    /// </summary>
    public class DeleteAccountInfoByIdCommand : CommandBase<Guid, Guid, DeleteCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="DeleteAccountInfoByIdCommand"/> instance
        /// </summary>
        /// <param name="id">id of the <see cref="AccountInfo"/> to delete.</param>
        public DeleteAccountInfoByIdCommand(Guid id) : base(Guid.NewGuid(), id)
        {
        }
    }
}

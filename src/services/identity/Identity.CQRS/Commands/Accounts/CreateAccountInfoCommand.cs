namespace Identity.CQRS.Commands.Accounts
{
    using Identity.DTO;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using Optional;

    using System;

    /// <summary>
    /// Command to create a new <see cref="AccountInfo"/>.
    /// </summary>
    /// <remarks>
    /// The comamnd either succeeds and gives back a <see cref="AccountInfo"/> instance or fails with a <see cref="CreateCommandFailure"/> outcome
    /// </remarks>
    public class CreateAccountInfoCommand : CommandBase<Guid, NewAccountInfo, Option<AccountInfo, CreateCommandFailure>>
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

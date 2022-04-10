namespace Identity.CQRS.Commands
{
    using Identity.ValueObjects;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using System;

    /// <summary>
    /// Command to invalidate an access token for an account.
    /// </summary>
    public class InvalidateAccessTokenByUsernameCommand : CommandBase<Guid, UserName, InvalidateAccessCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="InvalidateAccessTokenByUsernameCommand"/> instance
        /// </summary>
        /// <param name="username">username of the account to invalidate</param>
        public InvalidateAccessTokenByUsernameCommand(UserName username) : base(Guid.NewGuid(), username)
        {
            if (username == UserName.Empty)
            {
                throw new ArgumentException($"{nameof(username)} cannot be empty", nameof(username));
            }
        }
    }
}

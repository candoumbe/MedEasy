using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;

using System;

namespace Identity.CQRS.Commands
{
    /// <summary>
    /// Command to invalidate an access token for an account.
    /// </summary>
    public class InvalidateAccessTokenByUsernameCommand : CommandBase<Guid, string, InvalidateAccessCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="InvalidateAccessTokenByUsernameCommand"/> instance
        /// </summary>
        /// <param name="username">username of the account to invalidate</param>
        public InvalidateAccessTokenByUsernameCommand(string username) : base(Guid.NewGuid(), username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException(nameof(username), $"{nameof(username)} is null or whitespace");
            }
        }
    }
}

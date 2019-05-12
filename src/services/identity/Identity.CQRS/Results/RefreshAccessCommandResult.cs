namespace MedEasy.CQRS.Core.Commands.Results
{
    /// <summary>
    /// Type of result expected when invalidating an account access
    /// </summary>
    public enum RefreshAccessCommandResult
    {
        /// <summary>
        /// The command failed because a resource was not found
        /// </summary>
        NotFound,

        /// <summary>
        /// The command failed because of a possible conflict
        /// </summary>
        Conflict,

        /// <summary>
        /// The refresh token is no longer valid a the client must re authenticate itself
        /// </summary>
        Unauthorized
    }
}

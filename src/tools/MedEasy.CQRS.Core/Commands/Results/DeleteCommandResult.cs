namespace MedEasy.CQRS.Core.Commands.Results
{
    /// <summary>
    /// Type of result expected when deleting something
    /// </summary>
    public enum DeleteCommandResult
    {
        /// <summary>
        /// The command completed successfully
        /// </summary>
        Done,
        /// <summary>
        /// The command failed due to authorization policy
        /// </summary>
        Failed_Unauthorized,
        /// <summary>
        /// The command failed because a resource was not found
        /// </summary>
        Failed_NotFound,
        /// <summary>
        /// The command failed because of a possible conflict
        /// </summary>
        Failed_Conflict
    }
}

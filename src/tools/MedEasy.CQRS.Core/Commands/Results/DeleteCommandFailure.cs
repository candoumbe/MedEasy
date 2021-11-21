namespace MedEasy.CQRS.Core.Commands.Results
{
    /// <summary>
    /// Possible outcomes when a command to delete a resource could not be done
    /// </summary>
    public enum DeleteCommandFailure
    {
        /// <summary>
        /// The command failed due to authorization policy
        /// </summary>
        Unauthorized,

        /// <summary>
        /// The command failed because a resource was not found
        /// </summary>
        NotFound,

        /// <summary>
        /// The command failed because of a possible conflict
        /// </summary>
        Conflict
    }
}

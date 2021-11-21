namespace MedEasy.CQRS.Core.Commands.Results
{
    /// <summary>
    /// Possible outcomes when trying to create a new resource
    /// </summary>
    public enum CreateCommandResult
    {
        /// <summary>
        /// The command complete successfully
        /// </summary>
        Done,
        /// <summary>
        /// Command failed because of a (potential) conflict
        /// </summary>
        Failed_Conflict,
        /// <summary>
        /// Command failed because of a (related) resource not found
        /// </summary>
        Failed_NotFound,
        /// <summary>
        /// Command failed because of restriction
        /// </summary>
        Failed_Unauthorized
    }
}

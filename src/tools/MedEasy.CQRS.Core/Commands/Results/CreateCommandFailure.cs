namespace MedEasy.CQRS.Core.Commands.Results
{
    /// <summary>
    /// Possible failure outcome when a command to create a new resource fails
    /// </summary>
    public enum CreateCommandFailure
    {
        /// <summary>
        /// Command failed because of a (potential) conflict
        /// </summary>
        Conflict,
        /// <summary>
        /// Command failed because of a (related) resource not found
        /// </summary>
        NotFound,
        /// <summary>
        /// Command failed because of restriction
        /// </summary>
        Unauthorized
    }
}

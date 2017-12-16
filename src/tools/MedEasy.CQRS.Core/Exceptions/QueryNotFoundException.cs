namespace MedEasy.CQRS.Core.Exceptions
{
    /// <summary>
    /// Exception to throw when a resource/data is not found.
    /// </summary>
    /// <remarks>
    /// This type of exception should be thrown when creating a resource that point to a resource that doesn't exist and that relation is mandatory.
    /// </remarks>
    public class QueryNotFoundException : QueryException
    {
        /// <summary>
        /// Builds a new <see cref="QueryNotFoundException"/> instance
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public QueryNotFoundException(string message) : base(message)
        {

        }
    }
}

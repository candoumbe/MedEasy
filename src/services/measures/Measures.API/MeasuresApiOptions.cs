namespace Measures.API
{
    /// <summary>
    /// Gives strongly typed access to API's options.
    /// </summary>
    public class MeasuresApiOptions
    {
        /// <summary>
        /// Number of items to return when requiring a page of result
        /// </summary>
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Number of items the API can return in a single call at most
        /// </summary>
        public int MaxPageSize { get; set; }
    }
}

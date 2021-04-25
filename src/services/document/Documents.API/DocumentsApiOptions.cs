namespace Documents.API
{
    /// <summary>
    /// Wraps options that can affect the behavior of the API
    /// </summary>
    public class DocumentsApiOptions
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

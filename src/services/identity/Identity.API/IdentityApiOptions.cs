namespace Identity.API
{
    /// <summary>
    /// Options that change the API behavior
    /// </summary>
    public class IdentityApiOptions
    {
        /// <summary>
        /// Gets/sets the maximum number of elements that an endpoint can return at once.
        /// </summary>
        public int MaxPageSize { get; set; }

        /// <summary>
        /// Gets/sets the number of elements that an endpoint returns when no page size was specified.
        /// </summary>
        public int DefaultPageSize { get; set; }
    }
}
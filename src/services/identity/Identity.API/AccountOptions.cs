namespace Identity.API
{
    /// <summary>
    /// Wraps accounts that must be pre configured in the application.
    /// </summary>
    public record AccountOptions
    {
        /// <summary>
        /// Accounts configured using options
        /// </summary>
        public SystemAccount[] Accounts { get; set; }
    }
}

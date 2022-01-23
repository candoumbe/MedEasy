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

    /// <summary>
    /// An account that should be created on application' startup
    /// </summary>
    public record SystemAccount
    {
        /// <summary>
        /// Username of the account
        /// </summary>
        public string Username { get; init; }

        /// <summary>
        /// Email of the account
        /// </summary>
        public string Email { get; init; }

        /// <summary>
        /// Password that can be used to logged into the application with the current account
        /// </summary>
        public string Password { get; set; }

    }
}

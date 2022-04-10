namespace Identity.API
{
    using Identity.ValueObjects;

    /// <summary>
    /// An account that should be created on application' startup
    /// </summary>
    public record SystemAccount
    {
        /// <summary>
        /// Username of the account
        /// </summary>
        public UserName Username { get; init; }

        /// <summary>
        /// Email of the account
        /// </summary>
        public Email Email { get; init; }

        /// <summary>
        /// Password that can be used to logged into the application with the current account
        /// </summary>
        public string Password { get; set; }

    }
}

namespace Identity.DTO
{
    /// <summary>
    /// Custom claims that can be used throughout the application when parsing claims embedded in tokens.
    /// </summary>
    public static class CustomClaimTypes
    {
        /// <summary>
        /// Name of the claim that holds the account id
        /// </summary>
        public static readonly string AccountId = "account-id";
        /// <summary>
        /// Location (name, IP adress,  of GPS coordinates)
        /// </summary>
        public static readonly string Location = nameof(Location);

        /// <summary>
        /// Preferred time zone to use when displaying with <see cref="System.DateTime"/> related data to user
        /// </summary>
        public static readonly string TimeZoneId = nameof(TimeZoneId);
    }
}

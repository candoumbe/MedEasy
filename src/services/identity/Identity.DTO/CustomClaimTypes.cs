namespace Identity.DTO
{
    /// <summary>
    /// Custom claims that can be used throughout the application
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
    }
}

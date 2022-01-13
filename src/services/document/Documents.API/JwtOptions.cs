namespace Documents.API
{
    /// <summary>
    /// JWO potions
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets/sets the "audience" of the token
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets/sets the issuer of the token
        /// </summary>
        public string Issuer { get; set; }
    }
}

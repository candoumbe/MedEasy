namespace Measures.API.Features.Auth
{
    /// <summary>
    /// Wrapper for JWT authentication
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Key used to signed a token
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Issuer of the JWT
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Audience of the JWT
        /// </summary>
        public string Audience { get; set; }
    }
}

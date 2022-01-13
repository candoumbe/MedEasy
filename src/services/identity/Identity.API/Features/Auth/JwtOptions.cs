namespace Identity.API.Features.Auth
{
    using System.Collections.Generic;

    /// <summary>
    /// Wraps JWT Options.
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Access token lifetime (in minutes)
        /// </summary>
        public double AccessTokenLifetime { get; set; }
        /// <summary>
        /// Refresh token lifetime (in minutes)
        /// </summary>
        public double RefreshTokenLifetime { get; set; }

        /// <summary>
        /// Key used to signed a token
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Issuer of the token
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Expected audiences that the token is build for.
        /// </summary>
        public IEnumerable<string> Audiences { get; set; }
    }
}

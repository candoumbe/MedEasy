namespace Identity.DTO
{
    using System.Collections.Generic;

    /// <summary>
    /// Wraps JWT Token metadata (validity, issuer, ...)
    /// </summary>
    public sealed class JwtInfos
    {
        /// <summary>
        /// Access token validaty (in minutes)
        /// </summary>
        public double AccessTokenLifetime { get; set; }

        /// <summary>
        /// Refresh token validity in minutes
        /// </summary>
        public double RefreshTokenLifetime { get; set; }

        public string Key { get; set; }

        public string Issuer { get; set; }

        public IEnumerable<string> Audiences { get; set; }
    }
}

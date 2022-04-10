namespace Identity.DTO
{
    using System.Collections.Generic;

    /// <summary>
    /// Wraps JWT Token metadata (validity, issuer, ...)
    /// </summary>
    public sealed record JwtInfos
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

        ///<inheritdoc/>
        public void Deconstruct(out string key, out string issuer, out IEnumerable<string> audiences, out double accessTokenLifetime, out double refreshTokenLifetime)
        {
            key = Key;
            issuer = Issuer;
            audiences = Audiences;
            accessTokenLifetime = AccessTokenLifetime;
            refreshTokenLifetime = RefreshTokenLifetime;
        }
    }
}

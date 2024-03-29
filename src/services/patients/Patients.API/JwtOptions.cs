﻿namespace Patients.API
{
    /// <summary>
    /// Options associated with JWT computation.
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
        /// Name of the service that created the token
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Lists of services the token can be used for
        /// </summary>
        public string Audience { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.DTO
{
    /// <summary>
    /// Wraps JWT Token metadata (validity, issuer, ...)
    /// </summary>
    public sealed class JwtInfos
    {
        /// <summary>
        /// Access token validaty (in minutes)
        /// </summary>
        public double AccessTokenValidity { get; set; }

        /// <summary>
        /// Refresh token validity in minutes
        /// </summary>
        public double RefreshTokenValidity { get; set; }

        public string Key { get; set; }

        public string Issuer { get; set; }

        public IEnumerable<string> Audiences { get; set; }
    }
}

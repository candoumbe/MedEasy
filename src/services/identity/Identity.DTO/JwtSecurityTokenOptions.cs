using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.DTO
{
    /// <summary>
    /// Wraps JWT Token metadata (validity, issuer, ...)
    /// </summary>
    public sealed class JwtSecurityTokenOptions
    {
        /// <summary>
        /// token lifetime (in minutes)
        /// </summary>
        public double LifetimeInMinutes { get; set; }

        public string Key { get; set; }

        public string Issuer { get; set; }

        public IEnumerable<string> Audiences { get; set; }
    }
}

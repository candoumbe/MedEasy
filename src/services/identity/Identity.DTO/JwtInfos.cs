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
        /// Validaty in minutes
        /// </summary>
        public int Validity { get; set; }

        public string Key { get; set; }


        public string Issuer { get; set; }

        public IEnumerable<string> Audiences { get; set; }
    }
}

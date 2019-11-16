using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.API.Features.Auth
{
    /// <summary>
    /// Options to configure JWT tokens
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
        /// Which service created the token
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Which services can consumes the token
        /// </summary>
        public IEnumerable<string> Audiences { get; set; }
    }
}

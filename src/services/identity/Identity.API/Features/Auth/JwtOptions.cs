using System.Collections.Generic;

namespace Identity.API.Features.Auth
{
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


        public string Issuer { get; set; }

        public IEnumerable<string> Audiences { get; set; }
    }
}

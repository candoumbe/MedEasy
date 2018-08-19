using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.DTO
{
    public class BearerTokenInfo
    {
        /// <summary>
        /// Token that can be used to access a resource
        /// </summary>
        /// <remarks>
        /// This token as a shorter lifetime than <see cref="RefreshToken"/>
        /// </remarks>
        public string AccessToken { get; set; }

        /// <summary>
        /// Token that can be used to get a new access token when <see cref="AccessToken"/> has expired
        /// </summary>
        /// <remarks>
        /// This token as a longer lifetime than <see cref="RefreshToken"/>
        /// </remarks>
        public string RefreshToken { get; set; }
    }
}

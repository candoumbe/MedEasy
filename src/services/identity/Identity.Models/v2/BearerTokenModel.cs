using System;

namespace Identity.Models.v2
{
    /// <summary>
    /// Wraps informations on tokens provided by the identity API
    /// </summary>
    public class BearerTokenModel
    {
        /// <summary>
        /// Token that can be used to access a resource
        /// </summary>
        /// <remarks>
        /// This token as usually a shorter lifetime than <see cref="RefreshToken"/>
        /// </remarks>
        public TokenModel AccessToken { get; set; }

        /// <summary>
        /// Token that can be used to get a new access token when <see cref="AccessToken"/> has expired
        /// </summary>
        /// <remarks>
        /// This token as a longer lifetime than <see cref="RefreshToken"/>
        /// </remarks>
        public TokenModel RefreshToken { get; set; }
    }
}

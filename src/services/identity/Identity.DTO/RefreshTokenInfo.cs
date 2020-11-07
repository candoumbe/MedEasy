namespace Identity.DTO.Auth
{
    public class RefreshAccessTokenInfo
    {
        /// <summary>
        /// Access token to refresh
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string AccessToken { get; set; }

        /// <summary>
        /// Token used to as for a new <see cref="AccessToken"/> to be generated.
        /// </summary>
        /// <remarks>
        /// This token as a longer lifetime than <see cref="RefreshToken"/>
        /// </remarks>
        public string RefreshToken { get; set; }
    }
}

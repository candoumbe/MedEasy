namespace Identity.API.Features.v1.Auth
{
    /// <summary>
    /// Wraps a token value
    /// </summary>
    public class TokenModel
    {
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Builds a new <see cref="TokenModel"/> instance.
        /// </summary>
        /// <param name="token"></param>
        public TokenModel(string token) => Token = token;
    }
}
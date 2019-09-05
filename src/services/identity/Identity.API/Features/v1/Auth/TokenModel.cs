namespace Identity.API.Features.v1.Auth
{
    /// <summary>
    /// Wraps a token value
    /// </summary>
    public class TokenModel
    {
        public string Token { get; }

        public TokenModel(string token) => Token = token;
    }
}
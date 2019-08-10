namespace Identity.API.Features.Auth.v1
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
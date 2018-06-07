namespace MedEasy.Identity.API.Features.Authentication
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
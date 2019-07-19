namespace Identity.Models.Auth
{
    /// <summary>
    /// Wraps a token value
    /// </summary>
    public class BearerTokenModel
    { 
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}
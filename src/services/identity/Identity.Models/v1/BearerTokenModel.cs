namespace Identity.Models.v1
{
    public class BearerTokenModel
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}

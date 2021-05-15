namespace Identity.DTO
{
    using Microsoft.IdentityModel.Tokens;

    public class AuthenticationTokenInfo
    {
        public SecurityToken AccessToken { get; set; }

        public SecurityToken RefreshToken { get; set; }
    }
}

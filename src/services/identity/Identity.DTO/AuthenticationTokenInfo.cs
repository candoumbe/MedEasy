using Microsoft.IdentityModel.Tokens;

namespace Identity.DTO
{
    public class AuthenticationTokenInfo
    {
        public SecurityToken AccessToken { get; set; }

        public SecurityToken RefreshToken { get; set; }
    }
}

using System.Collections.Generic;
using System.Text;

namespace Identity.Models.v2
{
    public class BearerTokenModel
    {
        public TokenModel AccessToken { get; set; }

        public TokenModel RefreshToken { get; set; }
    }
}

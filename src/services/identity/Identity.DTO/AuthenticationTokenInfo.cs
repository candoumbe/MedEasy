using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.DTO
{
    public class AuthenticationTokenInfo
    {
        public SecurityToken AccessToken { get; set; }

        public SecurityToken RefreshToken { get; set; }
    }
}

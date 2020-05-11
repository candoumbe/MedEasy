using System;

namespace Identity.Models.v2
{
    public class TokenModel
    {
        public string Token { get; set; }

        public DateTime Expires { get; set; }
    }
}

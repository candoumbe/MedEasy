using System;

namespace Identity.DTO.v2
{
    public class TokenInfo
    {
        public string Token { get; set; }

        public DateTime Expires { get; set; }
    }
}

namespace Identity.DTO.v2
{
    using System;

    public class TokenInfo
    {
        public string Token { get; set; }

        public DateTime Expires { get; set; }
    }
}

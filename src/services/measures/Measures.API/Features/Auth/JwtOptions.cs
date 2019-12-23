namespace Measures.API.Features.Auth
{
    public class JwtOptions
    {
        /// <summary>
        /// Key used to signed a token
        /// </summary>
        public string Key { get; set; }


        public string Issuer { get; set; }

        public string Audience { get; set; }
    }
}

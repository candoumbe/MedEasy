namespace Documents.API
{
    public static partial class ServiceCollectionExtensions
    {
        public class JwtOptions
        {
            public string Key { get; set; }

            public string Audience { get; set; }

            public string Issuer { get; set; }

        }
    }
}

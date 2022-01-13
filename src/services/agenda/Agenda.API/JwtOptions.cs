namespace Agenda.API
{
    /// <summary>
    /// Wraps JWT Options
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Key
        /// </summary>
        /// <value></value>
        public string Key { get; set; }

        /// <summary>
        /// Audience
        /// </summary>
        /// <value></value>
        public string Audience { get; set; }

        /// <summary>
        /// Issuer of the JWT token
        /// </summary>
        /// <value></value>
        public string Issuer { get; set; }
    }
}

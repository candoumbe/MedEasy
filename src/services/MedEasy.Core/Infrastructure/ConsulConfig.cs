namespace MedEasy.Core.Infrastructure
{
    /// <summary>
    /// Holds consul configuration
    /// </summary>
    public class ConsulConfig
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Address of the consul server
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// ID of the service (should uniquely identifies an instance of a service
        /// </summary>
        public string ServiceID { get; set; }

        /// <summary>
        /// Tags associated with the services.
        /// </summary>
        public string[] Tags { get; set; }

        public ConsultCheckConfig Check { get; set; }
    }
    public class ConsultCheckConfig
    {
        public int Interval { get; set; }
        public int Timeout { get; set; }

        public string HealthEndpoint { get; set; }
    }
}

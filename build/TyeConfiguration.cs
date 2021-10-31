
namespace MedEasy.ContinuousIntegration
{
    using System.Collections.Generic;


    public class TyeConfiguration
    {
        /// <summary>
        /// Name of a configuration
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of services
        /// </summary>
        public List<TyeServiceConfiguration> Services { get; set; }
    }

}
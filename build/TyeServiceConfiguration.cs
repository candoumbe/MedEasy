
namespace MedEasy.ContinuousIntegration
{
    using System.Collections.Generic;

    /// <summary>
    /// Object representation of a service configured in a tye config file.
    /// </summary>
    public class TyeServiceConfiguration
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Docker image associated with the service
        /// </summary>
        public string Image { get; init; }

        /// <summary>
        /// Path to csproj file associated with the service (if any)
        /// </summary>
        public string Project { get; init; }

        /// <summary>
        /// Bindings associated with the service
        /// </summary>
        public List<TyeServiceBindingConfiguration> Bindings { get; init; }
    }

}
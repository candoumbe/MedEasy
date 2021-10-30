
namespace MedEasy.ContinuousIntegration
{
    using System.Collections.Generic;

    public class TyeServiceConfiguration
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public string Project { get; set; }

        public List<TyeServiceBindingConfiguration> Bindings { get; set; }
    }

}
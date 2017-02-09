using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// A ION resource
    /// </summary>
    public class IonResource : IIonResource
    {
        /// <summary>
        /// Metadata information about the resource
        /// </summary>
        public Link Meta { get; set; }
    }
}

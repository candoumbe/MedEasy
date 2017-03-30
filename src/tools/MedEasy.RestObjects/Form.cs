using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// Form representation
    /// </summary>
    /// <remarks>
    ///     This class, inspired by ION spec (see http://ionwg.org/draft-ion.html#forms for more details), 
    ///     can be used to describe a 
    /// </remarks>
    public class Form : IonResource
    {
        public IEnumerable<FormField> Items { get; set; }

        /// <summary>
        /// Builds a new <see cref="Form"/> instance.
        /// </summary>
        public Form()
        {
            Items = Enumerable.Empty<FormField>();
        }
    }
}


using MedEasy.RestObjects;
using System.Collections.Generic;

namespace MedEasy.API
{

    /// <summary>
    /// Describe an REST endpoint
    /// </summary>
    public class Endpoint
    {
        /// <summary>
        /// Name of the <see cref="Endpoint"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Link to the <see cref="Endpoint"/>
        /// </summary>
        public Link Link { get; set; }

        /// <summary>
        /// <see cref="Form"/>s associated with the <see cref="Endpoint"/>
        /// </summary>
        public IEnumerable<Form> Forms { get; set; }
    }
}

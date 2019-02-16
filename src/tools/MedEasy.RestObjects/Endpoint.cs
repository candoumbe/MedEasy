using System.Collections.Generic;
using System.Linq;

namespace MedEasy.RestObjects
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
        public IEnumerable<Form> Forms { get => _forms; set => _forms = value ?? Enumerable.Empty<Form>(); }

        private IEnumerable<Form> _forms;

        /// <summary>
        /// Builds a new <see cref="Endpoint"/> instance
        /// </summary>
        public Endpoint()
        {
            Forms = Enumerable.Empty<Form>();
        }
    }
}

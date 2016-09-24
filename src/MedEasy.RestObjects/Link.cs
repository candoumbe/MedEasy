using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.RestObjects
{

    /// <summary>
    /// Link representation
    /// </summary>
    public sealed class Link
    {
        /// <summary>
        /// Url of the resource
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// Relation of the link with the resource
        /// </summary>
        public string Rel { get; set; }

        public string Title { get; set; }
    }
}

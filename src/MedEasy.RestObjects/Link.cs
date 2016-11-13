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

        /// <summary>
        /// Http method needed to call <see cref="Href"/>
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Title associated with the link
        /// </summary>
        /// <remarks>
        /// Should be a friendly name suitable to used in a HTML a tag.
        /// </remarks>
        public string Title { get; set; }
    }
}

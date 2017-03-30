using System.Collections.Generic;
using System.Linq;

namespace MedEasy.RestObjects
{

    /// <summary>
    /// Link representation
    /// </summary>
    /// <remarks>
    ///     Inspired by ION spec (see http://ionwg.org/draft-ion.html#links for more details)
    /// </remarks>
    public class Link
    {
        /// <summary>
        /// Url of the resource the current <see cref="Link"/> points to.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Relation of the resource that <see cref="Link"/> points to with the current resource
        /// </summary>
        public string Relation { get; set; }

        /// <summary>
        /// Http method to used in conjunction with <see cref="Href"/>.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string Method { get; set; }

        /// <summary>
        /// Title associated with the link
        /// </summary>
        /// <remarks>
        /// Should be a friendly name suitable to used in a HTML a tag.
        /// </remarks>
        public string Title { get; set; }

        /// <summary>
        /// Indicates if the current <see cref="Href"/> is a template url
        /// </summary>
        /// <remarks>
        /// A template url is a url with generic placeholder.
        /// 
        /// <code>api/patients/{id?}</code> is a template url as it contiains one placeholder
        /// 
        /// </remarks>
        public bool? Template { get; set; }

    }
}

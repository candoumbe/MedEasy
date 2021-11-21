namespace MedEasy.RestObjects
{
    using System;

    /// <summary>
    /// Link representation
    /// </summary>
    /// <remarks>
    ///     Inspired by ION spec (see http://ionwg.org/draft-ion.html#links for more details)
    /// </remarks>
#if NETSTANDARD2_0
    public class Link
#else
    public record Link
#endif
    {
        /// <summary>
        /// Url of the resource the current <see cref="Link"/> points to.
        /// </summary>
        public string Href
        {
            get;
#if NETSTANDARD2_0_OR_GREATER
            set;
#elif NET5_0_OR_GREATER
            init;
#endif
        }

        /// <summary>
        /// Relation of the resource that <see cref="Link"/> points to with the current resource
        /// </summary>
        public string Relation
        {
            get;
#if NETSTANDARD2_0_OR_GREATER
            set;
#elif NET5_0_OR_GREATER
            init;
#endif
        }

        /// <summary>
        /// Http method to used in conjunction with <see cref="Href"/>.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string Method
        {
            get;
#if NETSTANDARD2_0_OR_GREATER
            set;
#elif NET5_0_OR_GREATER
            init;
#endif
        }

        /// <summary>
        /// Title associated with the link
        /// </summary>
        /// <remarks>
        /// Should be a friendly name suitable to used in a HTML a tag.
        /// </remarks>
        public string Title
        {
            get;
#if NETSTANDARD2_0_OR_GREATER
            set;
#elif NET5_0_OR_GREATER
            init;
#endif
        }


        /// <summary>
        /// Indicates if the current <see cref="Href"/> is a template url
        /// </summary>
        /// <remarks>
        /// A template url is a url with generic placeholder.
        /// <c>api/patients/{id?}</c> is a template url as it contiains one placeholder
        /// </remarks>
        public bool? Template
        {
            get;
#if NETSTANDARD2_0_OR_GREATER
            set;
#elif NET5_0_OR_GREATER
            init;
#endif
        }

        ///<inheritdoc/>
        public override string ToString() => this.Jsonify(null);
    }
}

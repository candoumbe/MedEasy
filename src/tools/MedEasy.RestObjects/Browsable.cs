namespace MedEasy.RestObjects
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A wrapper of a <typeparamref name="T"/> resource with its current <see cref="Links"/>
    /// </summary>
    /// <typeparam name="T">Type of the resource that will be wrapped</typeparam>
    public class Browsable<T>
    {
        private IEnumerable<Link> _links;

        /// <summary>
        /// Location of the resource. Can be cached for further operations
        /// </summary>
        public IEnumerable<Link> Links
        {
            get => _links ?? Enumerable.Empty<Link>();
            set => _links = value ?? Enumerable.Empty<Link>();
        }

        /// <summary>
        /// The resource that can be later retrieve using the <see cref="Links"/> property
        /// </summary>
        public T Resource { get; set; }

        /// <summary>
        /// Builds a new <see cref="Browsable{T}"/> instance.
        /// </summary>
        public Browsable() => _links = Enumerable.Empty<Link>();
    }
}

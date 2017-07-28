using MedEasy.RestObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.DTO
{
    /// <summary>
    /// A wrapper of a <typeparamref name="T"/> resource with its current <see cref="Links"/>
    /// </summary>
    /// <typeparam name="T">Type of the resource that will be wrapped</typeparam>
    [JsonObject]
    public class BrowsableResource<T> : IBrowsableResource<T>
    {

        private IEnumerable<Link> _links;

        /// <summary>
        /// Location of the resource. Can be cached for further operations
        /// </summary>
        [JsonProperty]
        public IEnumerable<Link> Links
        {
            get
            {
                return _links ?? Enumerable.Empty<Link>();
            }

            set
            {
                _links = value ?? Enumerable.Empty<Link>();
            }
        }

        /// <summary>
        /// The resource that can be later retrieve using the <see cref="Links"/> property
        /// </summary>
        [JsonProperty]
        public T Resource { get; set; }

        /// <summary>
        /// Builds a new <see cref="BrowsableResource{T}"/> instance.
        /// </summary>
        public BrowsableResource()
        {
            _links = Enumerable.Empty<Link>();
        }
    }
}

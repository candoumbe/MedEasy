using MedEasy.RestObjects;
using Newtonsoft.Json;

namespace MedEasy.DTO
{
    /// <summary>
    /// A wrapper of a <typeparamref name="T"/> resource with its current <see cref="Location"/>
    /// </summary>
    /// <typeparam name="T">Type of the resource that will be wrapped</typeparam>
    [JsonObject]
    public class BrowsableResource<T> : IBrowsableResource<T>
    {
        /// <summary>
        /// Location of the resource. Can be cached for further operations
        /// </summary>
        [JsonProperty]
        public Link Location { get; set; }

        /// <summary>
        /// The resource that can be later retrieve using the <see cref="Location"/> property
        /// </summary>
        [JsonProperty]
        public T Resource { get; set; }
    }
}

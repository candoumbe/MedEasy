using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// Bazse class for a resource
    /// </summary>
    /// <typeparam name="T">Type of the identifier of the resource</typeparam>
    [JsonObject]
    public abstract class Resource<T> : IResource<T>
        where T : IEquatable<T>
    {
        [JsonProperty]
        public T Id { get; set; }

        /// <summary>
        /// Gets/sets when the resource was last modified
        /// </summary>
        [JsonProperty]
        public DateTimeOffset UpdatedDate { get; set; }

        /// <summary>
        /// Gets/sets when the resource was last modified
        /// </summary>
        [JsonProperty]
        public DateTimeOffset CreatedDate { get; set; }
    }
}

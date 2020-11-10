using System;
#if NETSTANDARD1_1
using Newtonsoft.Json;
#else
using System.Text.Json.Serialization;
#endif
namespace MedEasy.RestObjects
{
    /// <summary>
    /// Bazse class for a resource
    /// </summary>
    /// <typeparam name="T">Type of the identifier of the resource</typeparam>
#if NETSTANDARD1_1
    [JsonObject]
#endif
    public abstract class Resource<T> : IResource<T>
        where T : IEquatable<T>
    {
        public T Id { get; set; }

        /// <summary>
        /// Gets/sets when the resource was last modified
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// Gets/sets when the resource was last modified
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Bazse class for a resource
    /// </summary>
    /// <typeparam name="T">Type of the identifier of the resource</typeparam>
    public abstract class ResourceBase<T> : IResource<T>
        where T : IEquatable<T>
    {
        public T Id { get; set; }

        /// <summary>
        /// Gets/sets when the resource was last modified
        /// </summary>
        public DateTimeOffset? UpdatedDate { get; set; }

        /// <summary>
        /// Unique identifier of the resource
        /// </summary>
        public Guid UUID { get; set; }
    }
}

using MedEasy.RestObjects;
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
    public abstract class Resource<T> : IResource<T>, IIonResource
        where T : IEquatable<T>
    {
        public T Id { get; set; }

        /// <summary>
        /// Metadata on the resource
        /// </summary>
        public Link Meta { get; set; }

        /// <summary>
        /// Gets/sets when the resource was last modified
        /// </summary>
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}

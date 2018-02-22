using System;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// Bazse class for a resource
    /// </summary>
    /// <typeparam name="T">Type of the identifier of the resource</typeparam>
    public abstract class Resource<T> : IResource<T>
        where T : IEquatable<T>
    {
        public T Id { get; set; }

        /// <summary>
        /// Gets/sets when the resource was last modified
        /// </summary>
        public DateTimeOffset UpdatedDate { get; set; }
    }
}

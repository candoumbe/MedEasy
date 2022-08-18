namespace MedEasy.RestObjects
{
    using NodaTime;

    using System;

#if !NETSTANDARD2_0
    using DataFilters.AspNetCore.Attributes;
#endif

    /// <summary>
    /// Bazse class for a resource
    /// </summary>
    /// <typeparam name="T">Type of the identifier of the resource</typeparam>
    public abstract class Resource<T> : IResource<T>
        where T : IEquatable<T>
    {

        /// <summary>
        /// Identifier of the resource.
        /// </summary>
#if !NETSTANDARD2_0
        [Minimal] 
#endif
        public T Id { get; set; }

        /// </summary>
        /// Gets/sets when the resource was last modified
        /// <summary>
#if !NETSTANDARD2_0
        [Minimal]
#endif
        public Instant? UpdatedDate { get; set; }

        /// <summary>
        /// Gets/sets when the resource was last modified
        /// </summary>
#if !NETSTANDARD2_0
        [Minimal]
#endif
        public Instant? CreatedDate { get; set; }
    }
}

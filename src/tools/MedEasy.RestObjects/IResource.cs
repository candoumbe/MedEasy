namespace MedEasy.RestObjects
{
    using NodaTime;

    using System;

    /// <summary>
    /// Describes the properties a browsable resource must implements
    /// </summary>
    /// <typeparam name="T">Type of the resource identifier</typeparam>
    public interface IResource<out T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Id of the resource
        /// </summary>
        T Id { get; }

        /// <summary>
        /// Last time the resource was updated
        /// </summary>
        Instant? UpdatedDate { get; }
    }
}

using System;

namespace MedEasy.DTO
{
    /// <summary>
    /// Describes the properties a browsable resource must implements
    /// </summary>
    /// <typeparam name="T">Type of the resource identifier</typeparam>
    public interface IResource<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Id of the resource
        /// </summary>
        T Id { get; }

    }
}

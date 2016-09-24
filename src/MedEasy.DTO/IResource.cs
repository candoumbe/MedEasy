using MedEasy.RestObjects;
using System;

namespace MedEasy.DTO
{
    /// <summary>
    /// This interface describes the properties a browsable resource must implements
    /// </summary>
    public interface IResource<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Id of the resource
        /// </summary>
        T Id { get; }

    }
}

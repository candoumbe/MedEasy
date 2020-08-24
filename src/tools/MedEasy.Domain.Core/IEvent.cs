using System;

namespace MedEasy.Domain.Core
{
    /// <summary>
    /// Defines the shape of an event in a DDD.
    /// </summary>
    /// <typeparam name="T">Type of the aggregate identifier which this event is appli</typeparam>
    public interface IEvent<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Id of the aggregate the event is applied onto
        /// </summary>
        public T Id { get; }

        /// <summary>
        /// Version of the event
        /// </summary>
        public uint Version { get; }
    }
}

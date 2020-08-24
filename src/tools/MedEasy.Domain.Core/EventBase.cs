using System;

namespace MedEasy.Domain.Core
{
    public abstract class EventBase<T> : IEvent<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Id of the aggregate this event is raised for
        /// </summary>
        public T Id { get; }

        public abstract uint Version { get; }

        protected EventBase(T id)
        {
            Id = id;
        }

        
    }
}

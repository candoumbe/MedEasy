using System;
using System.Collections.Generic;

namespace MedEasy.Domain.Core
{
    /// <summary>
    /// Interface to implement by aggregates.
    /// </summary>
    /// <typeparam name="TId">Type of the aggregate identifier.</typeparam>
    public interface IAggregate<TId> where TId : IEquatable<TId>
    {
        public TId Id { get; }

        /// <summary>
        /// Pending events
        /// </summary>
        public IEnumerable<IEvent<TId>> Pending { get; }

        /// <summary>
        /// Removes uncommited events
        /// </summary>
        void ClearUncommitedEvents();

       /// <summary>
       /// Applies an event to the aggregate
       /// </summary>
       /// <param name="evt"></param>
       /// <exception cref="ArgumentNullException"><paramref name="evt"/> is <c>null</c></exception>
        void Apply(IEvent<TId> evt);

        /// <summary>
        /// Raise an event
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="evt"></param>
        void Raise<TEvent>(TEvent evt) where TEvent : IEvent<TId>;
    }
}

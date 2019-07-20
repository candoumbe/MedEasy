using MedEasy.CQRS.Core.Events;
using System;

namespace Measures.CQRS.Events
{
    /// <summary>
    /// Event sent when a new measure was created
    /// </summary>
    /// <typeparam name="TId">Type of the event identifier</typeparam>
    /// <typeparam name="TMeasure">Type of the measurement associated with the event</typeparam>
    public abstract class MeasureCreated<TId, TMeasure> : NotificationBase<TId, TMeasure>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Builds a new <see cref="MeasureCreated{TId, TMeasure}"/>
        /// </summary>
        /// <param name="eventId">Event's identifier. Should uniquely identifies an event accross the entire system</param>
        /// <param name="data">Data associated with the event</param>
        protected MeasureCreated(TId eventId, TMeasure data) : base(eventId, data)
        {}
    }
}

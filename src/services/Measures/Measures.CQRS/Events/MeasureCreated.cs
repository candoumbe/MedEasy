using MedEasy.CQRS.Core.Events;
using System;

namespace Measures.CQRS.Events
{
    /// <summary>
    /// Event sent when a new measure was created
    /// </summary>
    public abstract class MeasureCreated<TId, TMeasure> : NotificationBase<TId, TMeasure>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// 
        /// </summary>
        public TMeasure Measure { get; }
        /// <summary>
        /// Builds a new <see cref="MeasureCreated{TId, TMeasure}"/>
        /// </summary>
        /// <param name="measureId"></param>
        public MeasureCreated(TId id, TMeasure measure) : base(id, measure)
        {}

    }
}

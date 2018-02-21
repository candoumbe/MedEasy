using MedEasy.CQRS.Core.Events;
using System;

namespace Measures.CQRS.Events
{
    /// <summary>
    /// Event sent when a new measure was created
    /// </summary>
    public abstract class MeasureCreated<TId, TMeasureId> : NotificationBase<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// 
        /// </summary>
        public TMeasureId MeasureId { get; }
        /// <summary>
        /// Builds a new <see cref="MeasureCreated{TId, TMeasureId}"/>
        /// </summary>
        /// <param name="measureId"></param>
        public MeasureCreated(TId id, TMeasureId measureId) : base(id)
        {
            MeasureId = measureId;
        }

    }
}

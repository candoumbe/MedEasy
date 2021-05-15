namespace Measures.CQRS.Events
{
    using MedEasy.CQRS.Core.Events;

    using System;

    /// <summary>
    /// Event sent when a new measure was deleted
    /// </summary>
    public abstract class MeasureDeleted<TId, TMeasureId> : NotificationBase<TId, TMeasureId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Id of the measure.
        /// </summary>
        public TMeasureId MeasureId { get; }
        /// <summary>
        /// Builds a new <see cref="MeasureCreated{TId, TMeasureId}"/>
        /// </summary>
        /// <param name="measureId"></param>
        protected MeasureDeleted(TId id, TMeasureId measureId) : base(id, measureId)
        {
        }
    }
}

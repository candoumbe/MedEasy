namespace Measures.Objects
{
    using Measures.Ids;

    using MedEasy.Ids;
    using MedEasy.Objects;

    using NodaTime;

    using System;

    /// <summary>
    /// Base class of all physiological measurements.
    /// </summary>
    /// <typeparam name="TId">Type of the identifier</typeparam>
    public abstract class PhysiologicalMeasurement<TId> : AuditableEntity<TId, PhysiologicalMeasurement<TId>>
        where TId : StronglyTypedId<Guid>
    {
        public Subject Subject { get; }

        /// <summary>
        /// Patient for which the measure was made
        /// </summary>
        public SubjectId SubjectId { get; }

        /// <summary>
        /// When the measure was made
        /// </summary>
        public Instant DateOfMeasure { get; set; }

        protected PhysiologicalMeasurement(SubjectId subjectId, TId id, Instant dateOfMeasure) : base(id)
        {
            SubjectId = subjectId;
            DateOfMeasure = dateOfMeasure;
        }
    }
}

using Measures.Ids;

using MedEasy.Ids;
using MedEasy.Objects;

using NodaTime;

using System;

namespace Measures.Objects
{
    public abstract class PhysiologicalMeasurement<TId> : AuditableEntity<TId, PhysiologicalMeasurement<TId>>
        where TId : StronglyTypedId<Guid>
    {
        public Patient Patient { get; set; }

        /// <summary>
        /// Patient for which the measure was made
        /// </summary>
        public PatientId PatientId { get; set; }

        /// <summary>
        /// When the measure was made
        /// </summary>
        public Instant DateOfMeasure { get; set; }

        protected PhysiologicalMeasurement(PatientId patientId, TId id, Instant dateOfMeasure) : base(id)
        {
            PatientId = patientId;
            DateOfMeasure = dateOfMeasure;
        }
    }
}

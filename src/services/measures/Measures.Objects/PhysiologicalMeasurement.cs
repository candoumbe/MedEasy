using MedEasy.Objects;

using NodaTime;

using System;

namespace Measures.Objects
{
    public abstract class PhysiologicalMeasurement : AuditableEntity<Guid, PhysiologicalMeasurement>
    {
        public Patient Patient { get; set; }

        /// <summary>
        /// Patient for which the measure was made
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// When the measure was made
        /// </summary>
        public Instant DateOfMeasure { get; set; }

        protected PhysiologicalMeasurement(Guid patientId, Guid id, Instant dateOfMeasure) : base(id)
        {
            PatientId = patientId;
            DateOfMeasure = dateOfMeasure;
        }
    }
}

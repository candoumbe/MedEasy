using MedEasy.Objects;
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
        public DateTime DateOfMeasure { get; set; }

        protected static readonly string ValueIsOutsideRangeOfValidValuesMessage = $"The new value is outside the range [0 ; {float.MaxValue}] of valid values.";

        /// <summary>
        /// Builds a new <see cref="PhysiologicalMeasurement"/> instance
        /// </summary>
        /// <param name="id">id of the measurement.</param>
        /// <param name="patientId">id of the patient the measurement was taken from</param>
        /// <param name="dateOfMeasure">When the measure was taken.</param>
        protected PhysiologicalMeasurement(Guid id, Guid patientId, DateTime dateOfMeasure) : base(id)
        {
            if (patientId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(patientId));
            }

            if (dateOfMeasure == DateTime.MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(dateOfMeasure), dateOfMeasure, $"{nameof(dateOfMeasure)} must be set");
            }
            PatientId = patientId;
            DateOfMeasure = dateOfMeasure;
        }
    }
}

using System;

namespace MedEasy.Objects
{
    public abstract class PhysiologicalMeasurement : AuditableEntity<int, PhysiologicalMeasurement>
    {
        public Patient Patient { get; set; }

        /// <summary>
        /// Patient for which the measure was made
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// When the measure was made
        /// </summary>
        public DateTimeOffset DateOfMeasure { get; set; }
    }
}

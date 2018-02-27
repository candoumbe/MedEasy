using MedEasy.Objects;
using System;

namespace Measures.Objects
{
    public abstract class PhysiologicalMeasurement : AuditableEntity<int, PhysiologicalMeasurement>
    {
        public virtual Patient Patient { get; set; }

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

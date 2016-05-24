using System;

namespace MedEasy.Objects
{
    public abstract class PhysiologicalMeasurement : AuditableEntity<int, PhysiologicalMeasurement>
    {
        public Patient Patient { get; set; }

        public int PatientId { get; set; }

        public DateTime DateOfMeasurement { get; set; }
    }
}

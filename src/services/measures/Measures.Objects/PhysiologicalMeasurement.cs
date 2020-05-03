using MedEasy.Objects;
using System;

namespace Measures.Objects
{
    public abstract class PhysiologicalMeasurement : AuditableEntity<Guid, PhysiologicalMeasurement>
    {
        public virtual Patient Patient { get; set; }

        /// <summary>
        /// Patient for which the measure was made
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// When the measure was made
        /// </summary>
        public DateTime DateOfMeasure { get; set; }


        protected PhysiologicalMeasurement(Guid id, Guid patientId, DateTime dateOfMeasure)
            : base(id)
        {
            PatientId = patientId;
            DateOfMeasure = dateOfMeasure;
        }


        public void ChangePatientId(Guid newPatientId)
        {
            PatientId = newPatientId;
        }
    }
}

using System.Collections.Generic;

namespace MedEasy.Objects
{
    /// <summary>
    /// An instance of this class represents a consultation between a <see cref="Patient"/> and a <see cref="Doctor"/>
    /// </summary>
    public class Consultation : AuditableEntity<int, Consultation>
    {
        public int PatientId { get; set; }

        public Patient Patient { get; set; }


        public int DoctorId { get; set; }

        public Doctor Doctor { get; set; }

        public ICollection<Prescription> Prescriptions { get; set; }



    }
}

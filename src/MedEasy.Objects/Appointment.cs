using System;

namespace MedEasy.Objects
{
    /// <summary>
    /// An instance of this class stands for an appointment between a <see cref="Patient"/> and a <see cref="Doctor"/>
    /// </summary>
    public class Appointment : AuditableEntity<int, Appointment>
    {
        public DateTimeOffset StartDate { get; set; }

        public int Duration { get; set; }

        public Doctor Doctor { get; set; }

        public Patient Patient { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }
    }
}

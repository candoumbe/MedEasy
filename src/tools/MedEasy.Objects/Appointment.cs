using System;

namespace MedEasy.Objects
{
    /// <summary>
    /// An appointment between a <see cref="Objects.Patient"/> and a <see cref="Objects.Doctor"/>
    /// </summary>
    /// <remarks>
    /// The <see cref="Duration"/> is only used to be able to schedule 
    /// </remarks>
    public class Appointment : AuditableEntity<int, Appointment>
    {
        /// <summary>
        /// When the <see cref="Appointment"/> starts
        /// </summary>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// How long the <see cref="Appointment"/> last in minutes
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// <see cref="Objects.Doctor"/> of the <see cref="Appointment"/>
        /// </summary>
        public Doctor Doctor { get; set; }

        /// <summary>
        /// <see cref="Objects.Patient"/> of the <see cref="Appointment"/>
        /// </summary>
        public Patient Patient { get; set; }

        /// <summary>
        /// Id of the <see cref="Objects.Patient"/> of the <see cref="Appointment"/>
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Id of the <see cref="Objects.Doctor"/>
        /// </summary>
        public int DoctorId { get; set; }
    }
}

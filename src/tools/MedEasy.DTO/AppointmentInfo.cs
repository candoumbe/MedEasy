using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Resource that holds appointment informations.
    /// </summary>
    public class AppointmentInfo : Resource<Guid>
    {
        /// <summary>
        /// Date of the beginning of the appointment
        /// </summary>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// Duration
        /// </summary>
        public double Duration { get; set; }
        /// <summary>
        /// The patient of the appointment
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Doctor of the appointment
        /// </summary>
        public Guid DoctorId { get; set; }
    }
}

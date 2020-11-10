using Agenda.Models.v1.Attendees;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Agenda.Models.v1.Appointments
{
    /// <summary>
    /// An <see cref="Appointment"/> beetween two or more people
    /// </summary>

    public class AppointmentModel : Resource<Guid>
    {
        /// <summary>
        /// Location of the appointment
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Subject of the appointment
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Start date of the appointment
        /// </summary>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// End date of the appointment
        /// </summary>
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Participants
        /// </summary>
        public IEnumerable<AttendeeModel> Attendees { get; set; }
    }
}

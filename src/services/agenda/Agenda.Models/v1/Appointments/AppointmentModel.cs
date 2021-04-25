using Agenda.Ids;
using Agenda.Models.v1.Attendees;

using MedEasy.RestObjects;

using NodaTime;

using System.Collections.Generic;

namespace Agenda.Models.v1.Appointments
{
    /// <summary>
    /// An <see cref="Appointment"/> beetween two or more people
    /// </summary>

    public class AppointmentModel : Resource<AppointmentId>
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
        public ZonedDateTime StartDate { get; set; }

        /// <summary>
        /// End date of the appointment
        /// </summary>
        public ZonedDateTime EndDate { get; set; }

        /// <summary>
        /// Participants
        /// </summary>
        public IEnumerable<AttendeeModel> Attendees { get; set; }
    }
}

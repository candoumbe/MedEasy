using Agenda.Models.v1.Attendees;

using NodaTime;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Agenda.Models.v1.Appointments
{
    /// <summary>
    /// Contains data to create a new <see cref="Appointment"/> beetween two or more person
    /// </summary>
    public class NewAppointmentModel
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
        /// Participants of the appointment
        /// </summary>
        public IEnumerable<AttendeeModel> Attendees { get; set; }


        public NewAppointmentModel()
        {
            Attendees = Enumerable.Empty<AttendeeModel>();
        }

        public override string ToString() => this.Jsonify();
    }
}

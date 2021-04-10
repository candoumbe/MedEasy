using MedEasy.RestObjects;

using NodaTime;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Agenda.DTO
{
    /// <summary>
    /// An <see cref="Appointment"/> beetween two or more people
    /// </summary>
    public class AppointmentInfo : Resource<Guid>
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
        public Instant StartDate { get; set; }

        /// <summary>
        /// End date of the appointment
        /// </summary>
        public Instant EndDate { get; set; }

        /// <summary>
        /// Participants
        /// </summary>
        public IEnumerable<AttendeeInfo> Attendees { get; set; }
    }
}

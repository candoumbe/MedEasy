using System;
using System.Text.Json.Serialization;

namespace Agenda.Objects
{
    /// <summary>
    /// Relation between a <see cref="Attendee"/> and an <see cref="Appointment"/>
    /// </summary>
    public class AppointmentAttendee
    {
        public Attendee Attendee { get; set; }

        public Guid AttendeeId { get; set; }

        public Guid AppointmentId { get; set; }

        [JsonIgnore]
        public Appointment Appointment { get; set; }
    }
}

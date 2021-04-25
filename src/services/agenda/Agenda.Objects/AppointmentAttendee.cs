using Agenda.Ids;

using System.Text.Json.Serialization;

namespace Agenda.Objects
{
    /// <summary>
    /// Relation between a <see cref="Attendee"/> and an <see cref="Appointment"/>
    /// </summary>
    public class AppointmentAttendee
    {
        public Attendee Attendee { get; set; }

        public AttendeeId AttendeeId { get; set; }

        public AppointmentId AppointmentId { get; set; }

        [JsonIgnore]
        public Appointment Appointment { get; set; }
    }
}

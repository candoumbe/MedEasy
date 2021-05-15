namespace Agenda.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Newtonsoft.Json.JsonConvert;
    using static Newtonsoft.Json.NullValueHandling;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Contains data to create a new <see cref="Appointment"/> beetween two or more person
    /// </summary>
    public class NewAppointmentInfo : IEquatable<NewAppointmentInfo>
    {
        /// <summary>
        /// Location of the appointment
        /// </summary>
        [JsonProperty]
        public string Location { get; set; }

        /// <summary>
        /// Subject of the appointment
        /// </summary>
        [JsonProperty]
        public string Subject { get; set; }

        /// <summary>
        /// Start date of the appointment
        /// </summary>
        [JsonProperty]
        public ZonedDateTime StartDate { get; set; }

        /// <summary>
        /// End date of the appointment
        /// </summary>
        [JsonProperty]
        public ZonedDateTime EndDate { get; set; }

        /// <summary>
        /// Participants of the appointment
        /// </summary>
        [JsonProperty]
        public IEnumerable<AttendeeInfo> Attendees { get; set; }

        public NewAppointmentInfo()
        {
            Attendees = Enumerable.Empty<AttendeeInfo>();
        }

        public override bool Equals(object obj) => Equals(obj as NewAppointmentInfo);
        public bool Equals(NewAppointmentInfo other) => other != null
            && Location == other.Location
            && Subject == other.Subject
            && StartDate.Equals(other.StartDate)
            && EndDate.Equals(other.EndDate)
            && EqualityComparer<IEnumerable<AttendeeInfo>>.Default.Equals(Attendees, other.Attendees);

        public override int GetHashCode() => HashCode.Combine(Location, Subject, StartDate, EndDate, Attendees);

        public static bool operator ==(NewAppointmentInfo info1, NewAppointmentInfo info2)
        {
            return EqualityComparer<NewAppointmentInfo>.Default.Equals(info1, info2);
        }

        public static bool operator !=(NewAppointmentInfo first, NewAppointmentInfo second)
        {
            return !(first == second);
        }


        public override string ToString() => SerializeObject(this, new JsonSerializerSettings { NullValueHandling = Ignore });
    }
}

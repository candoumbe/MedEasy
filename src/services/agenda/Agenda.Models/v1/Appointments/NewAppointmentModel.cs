using Agenda.Models.v1.Attendees;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static Newtonsoft.Json.JsonConvert;
using static Newtonsoft.Json.NullValueHandling;

namespace Agenda.Models.v1.Appointments
{
    /// <summary>
    /// Contains data to create a new <see cref="Appointment"/> beetween two or more person
    /// </summary>
    [JsonObject]
    public class NewAppointmentModel : IEquatable<NewAppointmentModel>
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
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// End date of the appointment
        /// </summary>
        [JsonProperty]
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Participants of the appointment
        /// </summary>
        [JsonProperty]
        public IEnumerable<AttendeeModel> Attendees { get; set; }


        public NewAppointmentModel()
        {
            Attendees = Enumerable.Empty<AttendeeModel>();
        }

        public override bool Equals(object obj) => Equals(obj as NewAppointmentModel);

        public bool Equals(NewAppointmentModel other) => other != null
            && Location == other.Location
            && Subject == other.Subject
            && StartDate.Equals(other.StartDate)
            && EndDate.Equals(other.EndDate)
            && EqualityComparer<IEnumerable<AttendeeModel>>.Default.Equals(Attendees, other.Attendees);

        public override int GetHashCode()
        {
            int hashCode = -1527032563;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Location);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Subject);
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(StartDate);
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(EndDate);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<AttendeeModel>>.Default.GetHashCode(Attendees);
            return hashCode;
        }

        public static bool operator ==(NewAppointmentModel Model1, NewAppointmentModel Model2)
        {
            return EqualityComparer<NewAppointmentModel>.Default.Equals(Model1, Model2);
        }

        public static bool operator !=(NewAppointmentModel first, NewAppointmentModel second)
        {
            return !(first == second);
        }


        public override string ToString() => SerializeObject(this, new JsonSerializerSettings { NullValueHandling = Ignore });
    }
}

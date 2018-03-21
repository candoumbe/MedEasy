using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Agenda.DTO
{
    /// <summary>
    /// Contains data to create a new <see cref="Appointment"/> beetween two or more person
    /// </summary>
    [DataContract]
    public class NewAppointmentInfo : IEquatable<NewAppointmentInfo>
    {
        /// <summary>
        /// Location of the appointment
        /// </summary>
        [DataMember]
        public string Location { get; set; }

        /// <summary>
        /// Subject of the appointment
        /// </summary>
        [DataMember]
        public string Subject { get; set; }

        /// <summary>
        /// Start date of the appointment
        /// </summary>
        [DataMember]
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// End date of the appointment
        /// </summary>
        [DataMember]
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Participants of the appointment
        /// </summary>
        [DataMember]
        public IEnumerable<ParticipantInfo> Participants { get; set; }


        public NewAppointmentInfo()
        {
            Participants = Enumerable.Empty<ParticipantInfo>();
        }

        public override bool Equals(object obj) => Equals(obj as NewAppointmentInfo);
        public bool Equals(NewAppointmentInfo other) => other != null 
            && Location == other.Location 
            && Subject == other.Subject 
            && StartDate.Equals(other.StartDate) 
            && EndDate.Equals(other.EndDate) 
            && EqualityComparer<IEnumerable<ParticipantInfo>>.Default.Equals(Participants, other.Participants);

        public override int GetHashCode()
        {
            int hashCode = -1527032563;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Location);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Subject);
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(StartDate);
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(EndDate);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<ParticipantInfo>>.Default.GetHashCode(Participants);
            return hashCode;
        }

        public static bool operator ==(NewAppointmentInfo info1, NewAppointmentInfo info2)
        {
            return EqualityComparer<NewAppointmentInfo>.Default.Equals(info1, info2);
        }

        public static bool operator !=(NewAppointmentInfo first, NewAppointmentInfo second)
        {
            return !(first == second);
        }
    }
}

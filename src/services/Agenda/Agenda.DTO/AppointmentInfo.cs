using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Agenda.DTO
{
    /// <summary>
    /// An <see cref="Appointment"/> beetween two or more people
    /// </summary>
    [DataContract]
    public class AppointmentInfo : Resource<Guid>
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
        /// Participants
        /// </summary>
        [DataMember]
        public IEnumerable<ParticipantInfo> Participants { get; set; }
    }
}

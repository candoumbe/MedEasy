using MedEasy.DTO.Search;
using System;

namespace Agenda.DTO.Resources.Search
{
    /// <summary>
    /// Data to search <see cref="AppointmentInfo"/>s
    /// </summary>
    public class SearchAppointmentInfo : AbstractSearchInfo<AppointmentInfo>
    {
        /// <summary>
        /// Min start or end date
        /// </summary>
        public DateTimeOffset? From { get; set; }
        /// <summary>
        /// Max start or end date
        /// </summary>
        public DateTimeOffset? To { get; set; }
        /// <summary>
        /// Subject of the appointments
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Location of the appointment
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Participant names
        /// </summary>
        public string Participant { get; set; }
    }
}

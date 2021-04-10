using MedEasy.DTO.Search;

using NodaTime;

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
        public ZonedDateTime? From { get; set; }

        /// <summary>
        /// Max start or end date
        /// </summary>
        public ZonedDateTime? To { get; set; }

        /// <summary>
        /// Subject of the appointments
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Location of the appointment
        /// </summary>
        public string Location { get; set; }
    }
}

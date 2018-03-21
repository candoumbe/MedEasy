using MedEasy.Objects;

namespace Agenda.Objects
{
    /// <summary>
    /// Participant of a <see cref="Appointment"/>
    /// </summary>
    public class Participant : AuditableEntity<int, Participant>
    {
        /// <summary>
        /// Name of the participant
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Phone number of the participant
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Email of the participant
        /// </summary>
        public string Email { get; set; }
    }
}

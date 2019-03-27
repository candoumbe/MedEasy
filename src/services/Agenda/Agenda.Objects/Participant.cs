using MedEasy.Objects;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Agenda.Objects
{
    /// <summary>
    /// Participant of a <see cref="Appointment"/>
    /// </summary>
    public class Participant : AuditableEntity<int, Participant>
    {
        private string _name;

        /// <summary>
        /// Name of the participant
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value?.ToTitleCase() ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Phone number of the participant
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Email of the participant
        /// </summary>
        public string Email { get; set; }

        private readonly IList<AppointmentParticipant> _appointments;

        public IEnumerable<AppointmentParticipant> Appointments => _appointments;

        /// <summary>
        /// Builds a new <see cref="Participant"/> instance
        /// </summary>
        /// <param name="name">Name of the participant</param>
        public Participant(string name)
        {
            _appointments = new List<AppointmentParticipant>();
            Name = name;
        }

        public Participant() : this(string.Empty)
        {}
    }
}

using MedEasy.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Optional;

namespace Agenda.Objects
{
    /// <summary>
    /// An meeting wih a location and a subject
    /// </summary>
    public class Appointment : AuditableEntity<int, Appointment>
    {
        private IList<AppointmentParticipant> _participants;
        /// <summary>
        /// Location of the appointment
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Subject of the <see cref="Appointment"/>
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Start date of the appointment
        /// </summary>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// End date of the <see cref="Appointment"/>
        /// </summary>
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Participants of the <see cref="Appointment"/>
        /// </summary>
        public IEnumerable<AppointmentParticipant> Participants { get => _participants; }


        public Appointment()
        {
            _participants = new List<AppointmentParticipant>();
        }

        /// <summary>
        /// Adds a participant to the current <see cref=""/>
        /// </summary>
        /// <param name="participant">The participant to add</param>
        /// <returns><c>true</c> if <see cref="participant"/> was successfully added and <c>false</c> otherwise</returns>
        public void AddParticipant(Participant participant)
        {
            if (participant.UUID == default)
            {
                participant.UUID = Guid.NewGuid();
            }
            _participants.Add(new AppointmentParticipant { Participant = participant , Appointment = this});
            
        }

        /// <summary>
        /// Remove a participant
        /// </summary>
        /// <param name="participantId"></param>
        public void RemoveParticipant(Guid participantId)
        {
            Option<AppointmentParticipant> optionalParticipant = _participants.SingleOrDefault(x => x.Participant.UUID == participantId)
                .SomeNotNull();

            optionalParticipant.MatchSome((participant) => _participants.Remove(participant));
        }
    }
}

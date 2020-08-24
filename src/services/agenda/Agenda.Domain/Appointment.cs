using Agenda.Domain.Events;
using MedEasy.Domain.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenda.Domain
{
    public class Appointment : Aggregate<Guid>
    {
        private readonly ISet<Attendee> _attendees;

        /// <summary>
        /// Location of the appointment
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Subject of the <see cref="Appointment"/>
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// Start date of the appointment
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// End date of the <see cref="Appointment"/>
        /// </summary>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// Participants of the <see cref="Appointment"/>
        /// </summary>
        public IEnumerable<Attendee> Attendees => _attendees;

        public Status Status { get; }

        public Appointment(Guid id) : base(id)
        {
            On<AppointmentRescheduled>(evt =>
            {
                StartDate = evt.Start;
                EndDate = evt.End;
            });
        }
    }
}

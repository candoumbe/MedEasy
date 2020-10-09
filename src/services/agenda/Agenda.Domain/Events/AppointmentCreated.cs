using MedEasy.Domain.Core;

using System;
using System.Collections.Generic;

namespace Agenda.Domain.Events
{
    /// <summary>
    /// Indicates that an <see cref="Appointment"/> was created
    /// </summary>
    public class AppointmentCreated : EventBase<Guid>
    {
        /// <summary>
        /// Indicates when the appointment start
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        /// Indicates when the appointment ends
        /// </summary>
        public DateTime End { get; }
        public string Title { get; }

        /// <summary>
        /// Indicates who will attend the appointment
        /// </summary>
        public IEnumerable<Attendee> Attendees { get; }

        public override uint Version => 1;

        public AppointmentCreated(Guid appointmentId, DateTime start, DateTime end, string title, IEnumerable<Attendee> attendees) : base(appointmentId)
        {
            Start = start;
            End = end;
            Title = title;
            Attendees = attendees;
        }
    }
}

using MedEasy.Domain.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenda.Domain.Events
{
    /// <summary>
    /// Indicates that an <see cref="Appointment"/> was rescheduled
    /// </summary>
    public class AppointmentRescheduled : EventBase<Guid>
    {
        public DateTime Start { get; }

        public DateTime End { get; }

        public override uint Version => 1;

        public AppointmentRescheduled(Guid appointmentId, DateTime start, DateTime end) : base(appointmentId)
        {
            Start = start;
            End = end;
        }

    }
}

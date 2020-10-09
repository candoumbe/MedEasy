using MedEasy.Domain.Core;

using System;

namespace Agenda.Domain.Events
{
    /// <summary>
    /// Indicates that an <see cref="Appointment"/> was rescheduled
    /// </summary>
    public class AppointmentRescheduled : EventBase<Guid>
    {
        /// <summary>
        /// New start
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        /// New end
        /// </summary>
        public DateTime End { get; }

        public override uint Version => 1;

        public AppointmentRescheduled(Guid appointmentId, DateTime start, DateTime end) : base(appointmentId)
        {
            Start = start;
            End = end;
        }
    }
}

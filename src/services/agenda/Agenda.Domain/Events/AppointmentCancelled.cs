using MedEasy.Domain.Core;

using System;
using System.Collections.Generic;

namespace Agenda.Domain.Events
{
    /// <summary>
    /// Indicates that an <see cref="Appointment"/> was cancelled
    /// </summary>
    public class AppointmentCancelled : EventBase<Guid>
    {
        public override uint Version => 1;

        /// <summary>
        /// Identifies the person who cancelled the <see cref="Appointment"/>
        /// </summary>
        public Guid Initiator { get; }

        public AppointmentCancelled(Guid appointmentId, Guid initiator) : base(appointmentId)
        {
            Initiator = initiator;
        }
    }
}

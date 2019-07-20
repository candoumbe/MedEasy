using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenda.Objects
{
    /// <summary>
    /// Relation between a <see cref="Attendee"/> and an <see cref="Appointment"/>
    /// </summary>
    public class AppointmentAttendee
    {
        public Attendee Attendee { get; set; }

        public Guid AttendeeId { get; set; }

        public Guid AppointmentId { get; set; }

        public Appointment Appointment { get; set; }
    }
}

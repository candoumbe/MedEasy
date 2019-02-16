using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenda.Objects
{
    /// <summary>
    /// Relation between a <see cref="Participant"/> and an <see cref="Appointment"/>
    /// </summary>
    public class AppointmentParticipant
    {
        public Participant Participant { get; set; }
        public int ParticipantId { get; set; }

        public int AppointmentId { get; set; }

        public Appointment Appointment { get; set; }
    }
}

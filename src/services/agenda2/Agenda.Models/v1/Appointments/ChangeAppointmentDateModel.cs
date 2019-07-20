using MedEasy.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenda.Models.v1.Appointments
{
    /// <summary>
    /// Model used to change an <see cref="AppointmentModel"/>'s <see cref="AppointmentModel.StartDate"/> and <see cref="AppointmentModel.EndDate"/> 
    /// </summary>
    public class ChangeAppointmentDateModel
    {
        [RequireNonDefault]
        public Guid AppointmentId { get; set; }

        [RequireNonDefault]
        public DateTimeOffset NewStartDate { get; set; }

        [RequireNonDefault]
        public DateTimeOffset NewEndDate { get; set; }


    }
}

namespace Agenda.Models.v1.Appointments
{
    using MedEasy.Attributes;

    using System;

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

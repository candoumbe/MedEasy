namespace Agenda.CQRS.Features.Appointments.Queries
{
    using Agenda.DTO;
    using Agenda.Ids;

    using MedEasy.CQRS.Core.Queries;

    using Optional;

    using System;

    /// <summary>
    /// A query to get an <see cref="AppointmentInfo"/>
    /// </summary>
    public class GetOneAppointmentInfoByIdQuery : GetOneResourceQuery<Guid, AppointmentId, Option<AppointmentInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetOneAppointmentInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="appointmentId">id of the <see cref="AppointmentInfo"/> to retrieve</param>
        /// <exception cref="ArgumentException">if <paramref name="appointmentId"/> is <see cref="Guid.Empty"/></exception>
        public GetOneAppointmentInfoByIdQuery(AppointmentId appointmentId) : base(Guid.NewGuid(), appointmentId)
        {
        }
    }
}

using Agenda.DTO;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;
using System.Collections.Generic;

namespace Agenda.CQRS.Features.Appointments.Queries
{
    /// <summary>
    /// A query to get an <see cref="AppointmentInfo"/>
    /// </summary>
    public class GetOneAppointmentInfoByIdQuery : GetOneResourceQuery<Guid, Guid, Option<AppointmentInfo>>
    {

        /// <summary>
        /// Builds a new <see cref="GetOneAppointmentInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="appointmentId">id of the <see cref="AppointmentInfo"/> to retrieve</param>
        /// <exception cref="ArgumentException">if <paramref name="appointmentId"/> is <see cref="Guid.Empty"/></exception>
        public GetOneAppointmentInfoByIdQuery(Guid appointmentId) : base(Guid.NewGuid(), appointmentId)
        { 
        }
    }
}

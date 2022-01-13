namespace Agenda.CQRS.Features.Appointments.Queries
{
    using Agenda.DTO;

    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;
    using MedEasy.RestObjects;

    using System;

    /// <summary>
    /// A query to get a page of <see cref="AppointmentInfo"/>
    /// </summary>
    public class GetPageOfAppointmentInfoQuery : QueryBase<Guid, PaginationConfiguration, Page<AppointmentInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfAppointmentInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination"></param>
        public GetPageOfAppointmentInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}

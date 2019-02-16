using Agenda.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Appointments.Queries
{
    /// <summary>
    /// A query to get a page of <see cref="AppointmentInfo"/>
    /// </summary>
    public class GetPageOfAppointmentInfoQuery : IWantPageOf<Guid, AppointmentInfo>
    {
        public Guid Id { get; }

        public PaginationConfiguration Data { get; }

        /// <summary>
        /// Builds a new <see cref="GetPageOfAppointmentInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination"></param>
        public GetPageOfAppointmentInfoQuery(PaginationConfiguration pagination) => Data = pagination;
    }
}

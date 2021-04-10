using Agenda.DTO;

using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;

using System;

namespace Agenda.CQRS.Features.Participants.Queries
{
    /// <summary>
    /// A query to get a page of <see cref="AttendeeInfo"/>
    /// </summary>
    public class GetPageOfAttendeeInfoQuery : QueryBase<Guid, PaginationConfiguration, Page<AttendeeInfo>>, IWantPageOf<Guid, AttendeeInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfAttendeeInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination"></param>
        public GetPageOfAttendeeInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination) { }

        /// <summary>
        /// Builds a new <see cref="GetPageOfAttendeeInfoQuery"/> instance.
        /// </summary>
        /// <param name="page">index of the page of result</param>
        /// <param name="pageSize">number of items the result should return at most</param>
        public GetPageOfAttendeeInfoQuery(int page, int pageSize) : this(new PaginationConfiguration { Page = page, PageSize = pageSize })
        {

        }
    }
}

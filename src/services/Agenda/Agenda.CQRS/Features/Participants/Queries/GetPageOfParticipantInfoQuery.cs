using Agenda.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Participants.Queries
{
    /// <summary>
    /// A query to get a page of <see cref="ParticipantInfo"/>
    /// </summary>
    public class GetPageOfParticipantInfoQuery : QueryBase<Guid, PaginationConfiguration, Page<ParticipantInfo>>, IWantPageOf<Guid, ParticipantInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfParticipantInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination"></param>
        public GetPageOfParticipantInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination) { }

        /// <summary>
        /// Builds a new <see cref="GetPageOfParticipantInfoQuery"/> instance.
        /// </summary>
        /// <param name="page">index of the page of result</param>
        /// <param name="pageSize">number of items the result should return at most</param>
        public GetPageOfParticipantInfoQuery(int page, int pageSize) : this(new PaginationConfiguration { Page = page, PageSize = pageSize })
        {

        }
    }
}

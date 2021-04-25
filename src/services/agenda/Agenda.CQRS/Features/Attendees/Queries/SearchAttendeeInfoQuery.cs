using Agenda.DTO;
using Agenda.DTO.Resources.Search;

using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;

using System;

namespace Agenda.CQRS.Features.Participants.Queries
{
    /// <summary>
    /// Data to search <see cref="AttendeeInfo"/>.
    /// </summary>
    public class SearchAttendeeInfoQuery : QueryBase<Guid, SearchAttendeeInfo, Page<AttendeeInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="SearchAttendeeInfoQuery"/> instance.
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="ArgumentException">if <paramref name="data"/> is <c>null</c> </exception>
        public SearchAttendeeInfoQuery(SearchAttendeeInfo data) : base(Guid.NewGuid(), data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}

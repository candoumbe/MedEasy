namespace Agenda.CQRS.Features.Participants.Handlers
{
    using Agenda.CQRS.Features.Participants.Queries;
    using Agenda.DTO;
    using Agenda.DTO.Resources.Search;
    using Agenda.Objects;

    using DataFilters;

    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;
    using MedEasy.DTO.Search;

    using MediatR;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using static DataFilters.FilterLogic;

    /// <summary>
    /// Handler for <see cref="SearchAttendeeInfoQuery"/> queries
    /// </summary>
    public class HandleSearchAttendeeInfoQuery : IRequestHandler<SearchAttendeeInfoQuery, Page<AttendeeInfo>>
    {
        private readonly IHandleSearchQuery _handleSearch;

        /// <summary>
        /// Builds
        /// </summary>
        /// <param name="handleSearch"></param>
        public HandleSearchAttendeeInfoQuery(IHandleSearchQuery handleSearch)
        {
            _handleSearch = handleSearch;
        }

        ///<inheritdoc/>
        public async Task<Page<AttendeeInfo>> Handle(SearchAttendeeInfoQuery request, CancellationToken cancellationToken)
        {
            SearchAttendeeInfo requestData = request.Data;
            IList<IFilter> filters = new List<IFilter>(3);
            if (!string.IsNullOrWhiteSpace(requestData.Name))
            {
                filters.Add($"{nameof(AttendeeInfo.Name)}={requestData.Name}".ToFilter<AttendeeInfo>());
            }

            IFilter filter = filters.Count == 1
                ? filters.Single()
                : new MultiFilter { Logic = And, Filters = filters };

            SearchQueryInfo<AttendeeInfo> data = new()
            {
                Page = requestData.Page,
                PageSize = requestData.PageSize,
                Sort = requestData.Sort?.ToSort<AttendeeInfo>() ?? new Sort<AttendeeInfo>(nameof(AttendeeInfo.UpdatedDate), SortDirection.Descending),
                Filter = filter
            };

            SearchQuery<AttendeeInfo> query = new(data);

            return await _handleSearch.Search<Attendee, AttendeeInfo>(query, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

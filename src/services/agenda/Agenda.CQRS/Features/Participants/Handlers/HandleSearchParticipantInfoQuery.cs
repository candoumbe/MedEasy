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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DataFilters.FilterLogic;

namespace Agenda.CQRS.Features.Participants.Handlers
{
    /// <summary>
    /// Handler for <see cref="SearchParticipantInfoQuery"/> queries
    /// </summary>
    public class HandleSearchParticipantInfoQuery : IRequestHandler<SearchParticipantInfoQuery, Page<ParticipantInfo>>
    {
        private readonly IHandleSearchQuery _handleSearch;

        /// <summary>
        /// Builds 
        /// </summary>
        /// <param name="handleSearch"></param>
        public HandleSearchParticipantInfoQuery(IHandleSearchQuery handleSearch)
        {
            _handleSearch = handleSearch;
        }

        public async Task<Page<ParticipantInfo>> Handle(SearchParticipantInfoQuery request, CancellationToken cancellationToken)
        {
            SearchParticipantInfo requestData = request.Data;
            IList<IFilter> filters = new List<IFilter>(3);
            if (!string.IsNullOrWhiteSpace(requestData.Name))
            {
                filters.Add($"{nameof(ParticipantInfo.Name)}={requestData.Name}".ToFilter<ParticipantInfo>());
            }

            IFilter filter = filters.Count == 1
                ? filters.Single()
                : new CompositeFilter { Logic = And, Filters = filters };

            SearchQueryInfo<ParticipantInfo> data = new SearchQueryInfo<ParticipantInfo>
            {
                Page = requestData.Page,
                PageSize = requestData.PageSize,
                Sort = requestData.Sort?.ToSort<ParticipantInfo>() ?? new Sort<ParticipantInfo>(nameof(ParticipantInfo.UpdatedDate), SortDirection.Descending),
                Filter = filter
            };

            SearchQuery<ParticipantInfo> query = new SearchQuery<ParticipantInfo>(data);

            return await _handleSearch.Search<Participant, ParticipantInfo>(query, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

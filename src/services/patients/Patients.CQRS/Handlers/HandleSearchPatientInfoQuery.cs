namespace Patients.CQRS.Handlers
{
    using DataFilters;

    using global::Patients.CQRS.Queries;
    using global::Patients.DTO;

    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.DAL.Repositories;
    using MediatR;

    using System.Collections.Generic;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DTO.Search;
    using System.Linq;


    /// <summary>
    /// Handles <see cref="SearchQuery{PatientInfo}"/> queries
    /// </summary>
    public class HandleSearchPatientInfoQuery : IRequestHandler<SearchPatientInfoQuery, Page<PatientInfo>>
    {
        private readonly IHandleSearchQuery _handleSearchQuery;

        /// <summary>
        /// Builds a new <see cref="HandleSearchPatientInfoQuery"/> instance.
        /// </summary>
        public HandleSearchPatientInfoQuery(IHandleSearchQuery handleSearchQuery) 
        {
            _handleSearchQuery = handleSearchQuery;
        }

        ///<inheritdoc/>
        public async Task<Page<PatientInfo>> Handle(SearchPatientInfoQuery request, CancellationToken cancellationToken)
        {
            IList<IFilter> filters = new List<IFilter>();
            SearchPatientInfo search = request.Data;

            if (!string.IsNullOrEmpty(search.Firstname))
            {
                filters.Add($"{nameof(search.Firstname)}={search.Firstname}".ToFilter<PatientInfo>());
            }

            if (!string.IsNullOrEmpty(search.Lastname))
            {
                filters.Add($"{nameof(search.Lastname)}={search.Lastname}".ToFilter<PatientInfo>());
            }

            SearchQuery<PatientInfo> searchQuery = new(new SearchQueryInfo<PatientInfo>()
            {
                Filter = filters.Exactly(1)
                    ? filters.Single()
                    : new MultiFilter { Logic = FilterLogic.And, Filters = filters },
                Page = search.Page,
                PageSize = search.PageSize,
                Sort = search.Sort?.ToSort<PatientInfo>() ?? new Sort<PatientInfo>(nameof(PatientInfo.Lastname), SortDirection.Descending)
            });

            return await _handleSearchQuery.Search<Objects.Patient, PatientInfo>(searchQuery, cancellationToken)
                                           .ConfigureAwait(false);
}
    }
}

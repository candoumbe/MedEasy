namespace Documents.CQRS.Handlers
{
    using DataFilters;

    using Documents.CQRS.Queries;
    using Documents.DTO.v1;
    using Documents.Objects;

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
    /// Handles <see cref="SearchDocumentInfoQuery"/> queries.
    /// </summary>
    public class HandleSearchDocumentInfoQuery : IRequestHandler<SearchDocumentInfoQuery, Page<DocumentInfo>>
    {
        private readonly IHandleSearchQuery _handleSearchQuery;

        /// <summary>
        /// Builds a new <see cref="HandleSearchDocumentInfoQuery"/> instance.
        /// </summary>
        /// <param name="handleSearchQuery">Generic search service</param>
        public HandleSearchDocumentInfoQuery(IHandleSearchQuery handleSearchQuery)
        {
            _handleSearchQuery = handleSearchQuery;
        }

        ///<inheritdoc/>
        public async Task<Page<DocumentInfo>> Handle(SearchDocumentInfoQuery request, CancellationToken cancellationToken)
        {
            IList<IFilter> filters = new List<IFilter>();
            SearchDocumentInfo searchCriteria = request.Data;

            // TODO Replace with IFilterService
            if (!string.IsNullOrWhiteSpace(searchCriteria.Name))
            {
                filters.Add($"{nameof(Document.Name)}={searchCriteria.Name}".ToFilter<DocumentInfo>());
            }

            if (!string.IsNullOrWhiteSpace(searchCriteria.MimeType))
            {
                filters.Add($"{nameof(Document.MimeType)}={searchCriteria.MimeType}".ToFilter<DocumentInfo>());
            }

            SearchQueryInfo<DocumentInfo> searchQueryInfo = new()
            {
                Page = searchCriteria.Page,
                PageSize = searchCriteria.PageSize,
                Sort = searchCriteria.Sort?.ToSort<DocumentInfo>() ?? new Sort<DocumentInfo>(nameof(DocumentInfo.UpdatedDate))
            };
            if (filters.Count > 0)
            {
                searchQueryInfo.Filter = filters.Once()
                    ? filters.Single()
                    : new MultiFilter { Logic = And, Filters = filters };
            }

            return await _handleSearchQuery.Search<Document, DocumentInfo>(new SearchQuery<DocumentInfo>(searchQueryInfo), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

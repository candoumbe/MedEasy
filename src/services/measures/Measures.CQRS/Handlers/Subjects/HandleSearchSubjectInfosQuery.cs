namespace Measures.CQRS.Handlers.Subjects
{
    using Measures.DTO;
    using Measures.Objects;

    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;

    using MediatR;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="SearchQuery{SubjectInfo}"/> instances.
    /// </summary>
    public class HandleSearchSubjectInfosQuery : IRequestHandler<SearchQuery<SubjectInfo>, Page<SubjectInfo>>
    {
        private readonly IHandleSearchQuery _handleSearchQuery;

        /// <summary>
        /// Builds a new <see cref="HandleSearchSubjectInfosQuery"/> instance.
        /// </summary>
        /// <param name="handleSearchQuery">handler for all search queries.</param>
        public HandleSearchSubjectInfosQuery(IHandleSearchQuery handleSearchQuery)
        {
            _handleSearchQuery = handleSearchQuery ?? throw new ArgumentNullException(nameof(handleSearchQuery));
        }

        ///<inheritdoc/>
        public async Task<Page<SubjectInfo>> Handle(SearchQuery<SubjectInfo> request, CancellationToken cancellationToken)
            => await _handleSearchQuery.Search<Subject, SubjectInfo>(request, cancellationToken).ConfigureAwait(false);
    }
}

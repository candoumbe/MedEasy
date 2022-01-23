namespace Measures.CQRS.Handlers.BloodPressures
{
    using Measures.DTO;
    using Measures.Objects;

    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;

    using MediatR;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handler for <see cref="SearchQuery{BloodPressureInfo}"/> queries.
    /// </summary>
    public class HandleSearchBloodPressureInfosQuery : IRequestHandler<SearchQuery<BloodPressureInfo>, Page<BloodPressureInfo>>
    {
        private readonly IHandleSearchQuery _handleSearchQuery;

        public HandleSearchBloodPressureInfosQuery(IHandleSearchQuery handleSearchQuery)
        {
            _handleSearchQuery = handleSearchQuery;
        }

        ///<inheritdoc/>
        public async Task<Page<BloodPressureInfo>> Handle(SearchQuery<BloodPressureInfo> request, CancellationToken cancellationToken)
            => await _handleSearchQuery.Search<BloodPressure, BloodPressureInfo>(request, cancellationToken)
                                       .ConfigureAwait(false);
    }
}

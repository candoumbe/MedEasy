using Measures.DTO;
using Measures.Objects;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers
{
    public class HandleSearchBloodPressureInfosQuery : IRequestHandler<SearchQuery<BloodPressureInfo>, Page<BloodPressureInfo>>
    {
        private readonly IHandleSearchQuery _handleSearchQuery;

        public HandleSearchBloodPressureInfosQuery(IHandleSearchQuery handleSearchQuery)
        {
            _handleSearchQuery = handleSearchQuery;
        }

        public async Task<Page<BloodPressureInfo>> Handle(SearchQuery<BloodPressureInfo> request, CancellationToken cancellationToken)
        {
            Page<BloodPressureInfo> page = await _handleSearchQuery.Search<BloodPressure, BloodPressureInfo>(request, cancellationToken)
                .ConfigureAwait(false);

            return page;

        } 
    }
}

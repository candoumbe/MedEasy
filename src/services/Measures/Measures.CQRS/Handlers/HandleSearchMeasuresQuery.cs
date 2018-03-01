using AutoMapper.QueryableExtensions;
using Measures.DTO;
using Measures.Objects;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers
{
    public class HandleSearchMeasuresQuery<TMeasure, TMeasureInfo> :  HandleSearchQuery
        where TMeasure : PhysiologicalMeasurement
        where TMeasureInfo : PhysiologicalMeasurementInfo
    {
        public HandleSearchMeasuresQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, ILogger<HandleSearchQuery> logger) : base(uowFactory, expressionBuilder)
        {
        }

        public async Task<Page<TMeasureInfo>> Search(SearchQuery<TMeasureInfo> request, CancellationToken cancellationToken) => 
            await Search<TMeasure, TMeasureInfo>(request, cancellationToken)
                .ConfigureAwait(false);
    }
}

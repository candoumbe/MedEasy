namespace Measures.CQRS.Handlers
{
    using AutoMapper.QueryableExtensions;

    using Measures.DTO;
    using Measures.Objects;

    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.Ids;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class HandleSearchMeasuresQuery<TMeasureId, TMeasure, TMeasureInfo> : HandleSearchQuery
        where TMeasure : PhysiologicalMeasurement<TMeasureId>
        where TMeasureInfo : PhysiologicalMeasurementInfo<TMeasureId>
        where TMeasureId : StronglyTypedId<Guid>, IEquatable<TMeasureId>
    {
        public HandleSearchMeasuresQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, ILogger<HandleSearchQuery> logger) : base(uowFactory, expressionBuilder)
        {
        }

        public Task<Page<TMeasureInfo>> Search(SearchQuery<TMeasureInfo> request, CancellationToken cancellationToken) =>
            Search<TMeasure, TMeasureInfo>(request, cancellationToken);
    }
}

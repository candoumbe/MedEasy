using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using AutoMapper.QueryableExtensions;
using MedEasy.Queries;
using System.Threading.Tasks;
using MedEasy.Objects;
using System.Linq.Expressions;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Handlers.Core.Patient.Queries;
using System.Threading;
using Optional;

namespace MedEasy.Handlers.Patient.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOneResource{TQueryId, TData, TResult}"/> interface implementations
    /// </summary
    public class HandleGetOnePhysiologicalMeasurementInfoQuery<TPhysiologicalMeasurementEntity, TPhysiologicalMeasurementInfo> : OneResourceQueryHandlerBase<Guid, TPhysiologicalMeasurementEntity, GetOnePhysiologicalMeasureInfo, TPhysiologicalMeasurementInfo, IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TPhysiologicalMeasurementInfo>>, IHandleGetOnePhysiologicalMeasureQuery<TPhysiologicalMeasurementInfo>
        where TPhysiologicalMeasurementEntity : PhysiologicalMeasurement
        where TPhysiologicalMeasurementInfo : PhysiologicalMeasurementInfo
    {
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetOnePhysiologicalMeasurementInfoQuery{TPhysiologicalMeasurementEntity, TPhysiologicalMeasurementInfo}"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Patient"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder">Builder for <see cref="System.Linq.Expressions.Expression{TDelegate}"/>that can map <see cref="Objects.Temperature"/> instances to <see cref="TemperatureInfo"/> instances</param>
        public HandleGetOnePhysiologicalMeasurementInfoQuery(IUnitOfWorkFactory factory, ILogger<HandleGetOnePhysiologicalMeasurementInfoQuery<TPhysiologicalMeasurementEntity, TPhysiologicalMeasurementInfo>> logger, IExpressionBuilder expressionBuilder) : base(factory)
        {
            _expressionBuilder = expressionBuilder;
        }

        public override async ValueTask<Option<TPhysiologicalMeasurementInfo>> HandleAsync(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TPhysiologicalMeasurementInfo> query, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (IUnitOfWork uow = UowFactory.New())
            {
                Expression<Func<TPhysiologicalMeasurementEntity, TPhysiologicalMeasurementInfo>> selector = _expressionBuilder.GetMapExpression<TPhysiologicalMeasurementEntity, TPhysiologicalMeasurementInfo>();

                Option<TPhysiologicalMeasurementInfo> result = await uow.Repository<TPhysiologicalMeasurementEntity>()
                    .SingleOrDefaultAsync(
                        selector, 
                        x => x.Patient.UUID == query.Data.PatientId && x.UUID == query.Data.MeasureId, 
                        cancellationToken);


                return result;
            }
        }
    }
}
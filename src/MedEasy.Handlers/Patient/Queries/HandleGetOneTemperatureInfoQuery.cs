using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using MedEasy.Validators;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using System.Threading.Tasks;
using MedEasy.Objects;
using System.Linq.Expressions;

namespace MedEasy.Handlers.Patient.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOneResource{TQueryId, TData, TResult}"/> interface implementations
    /// </summary
    public class HandleGetOneTemperatureInfoQuery : QueryHandlerBase<Guid, Temperature, GetOnePhysiologicalMeasureInfo, TemperatureInfo, IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo>>, IHandleGetOneTemperatureQuery
    {
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetOneTemperatureInfoQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Patient"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder">Builder for <see cref="System.Linq.Expressions.Expression{TDelegate}"/>that can map <see cref="Objects.Temperature"/> instances to <see cref="TemperatureInfo"/> instances</param>
        public HandleGetOneTemperatureInfoQuery(IUnitOfWorkFactory factory, ILogger<HandleGetOneTemperatureInfoQuery> logger, IExpressionBuilder expressionBuilder) : base(factory)
        {
            _expressionBuilder = expressionBuilder;
        }

        public override async Task<TemperatureInfo> HandleAsync(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (var uow = UowFactory.New())
            {
                Expression<Func<Temperature, TemperatureInfo>> selector = _expressionBuilder.CreateMapExpression<Temperature, TemperatureInfo>();

                TemperatureInfo result = await uow.Repository<Temperature>()
                    .SingleOrDefaultAsync(selector, x => x.PatientId == query.Data.PatientId && x.Id == query.Data.MeasureId);


                return result;
            }
        }
    }
}
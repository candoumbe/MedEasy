using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Prescription.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOneResource{TQueryId, TData, TResult}"/> interface implementations
    /// </summary
    public class HandleGetOnePrescriptionHeaderInfoQuery : GenericGetOneByIdQueryHandler<Guid, Objects.Prescription, int, PrescriptionHeaderInfo, IWantOneResource<Guid, int, PrescriptionHeaderInfo>>
    {
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetOnePhysiologicalMeasurementInfoQuery{TPhysiologicalMeasurementEntity, TPhysiologicalMeasurementInfo}"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Prescription"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder">Builder for <see cref="Expression{TDelegate}"/>that can map <see cref="Objects.Temperature"/> instances to <see cref="TemperatureInfo"/> instances</param>
        public HandleGetOnePrescriptionHeaderInfoQuery(IUnitOfWorkFactory factory, ILogger<HandleGetOnePrescriptionHeaderInfoQuery> logger, IExpressionBuilder expressionBuilder) : base(logger, factory, expressionBuilder)
        {
        }

    }
}
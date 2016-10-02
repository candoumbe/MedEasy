using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Patient.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOnePatientInfoByIdQuery"/> interface implementations
    /// </summary
    public class HandleGetManyPatientInfoQuery : GenericGetManyQueryHandler<Guid, Objects.Patient, int, PatientInfo, IWantManyResources<Guid, PatientInfo>>, IHandleGetManyPatientInfosQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetPatientInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Patient"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder">Builder for <see cref="System.Linq.Expressions.Expression{TDelegate}"/>that can map <see cref="Objects.Patient"/> instances to <see cref="PatientInfo"/> instances</param>
        public HandleGetManyPatientInfoQuery(IUnitOfWorkFactory factory, ILogger<HandleGetManyPatientInfoQuery> logger, IExpressionBuilder expressionBuilder) : base(logger, factory, expressionBuilder)
        {
        }
    }
}
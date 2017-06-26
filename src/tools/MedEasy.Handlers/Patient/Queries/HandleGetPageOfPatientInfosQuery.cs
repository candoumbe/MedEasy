using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using AutoMapper.QueryableExtensions;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Handlers.Core.Patient.Queries;

namespace MedEasy.Handlers.Patient.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantPageOfResources{Guid, PatientInfo}"/> interface implementations
    /// </summary
    public class HandleGetPageOfPatientInfoQuery : PagedResourcesQueryHandlerBase<Guid, Objects.Patient, PatientInfo, IWantPageOfResources<Guid, PatientInfo>>, IHandleGetPageOfPatientInfosQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetPatientInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Patient"/> instances</param>
        /// <param name="expressionBuilder">Builder for <see cref="System.Linq.Expressions.Expression{TDelegate}"/>that can map <see cref="Objects.Patient"/> instances to <see cref="PatientInfo"/> instances</param>
        public HandleGetPageOfPatientInfoQuery(IUnitOfWorkFactory factory, ILogger<HandleGetPageOfPatientInfoQuery> logger, IExpressionBuilder expressionBuilder) : base(factory, expressionBuilder)
        {
        }
    }
}
using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using AutoMapper.QueryableExtensions;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Handlers.Core.Prescription.Queries;

namespace MedEasy.Handlers.Prescription.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOnePrescriptionInfoByIdQuery"/> interface implementations
    /// </summary
    public class HandleGetManyPrescriptionHeaderInfoQuery : GenericGetManyQueryHandler<Guid, Objects.Prescription, int, PrescriptionHeaderInfo, IWantManyResources<Guid, PrescriptionHeaderInfo>>, IHandleGetManyPrescriptionHeaderInfosQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetManyPrescriptionHeaderInfoQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Prescription"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder">Builder for <see cref="System.Linq.Expressions.Expression{TDelegate}"/>that can map <see cref="Objects.Prescription"/> instances to <see cref="PrescriptionInfo"/> instances</param>
        public HandleGetManyPrescriptionHeaderInfoQuery(IUnitOfWorkFactory factory, ILogger<HandleGetManyPrescriptionHeaderInfoQuery> logger, IExpressionBuilder expressionBuilder) : base(logger, factory, expressionBuilder)
        {
        }
    }
}
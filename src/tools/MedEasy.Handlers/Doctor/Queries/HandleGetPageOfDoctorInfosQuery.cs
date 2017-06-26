using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using AutoMapper.QueryableExtensions;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Handlers.Core.Doctor.Queries;

namespace MedEasy.Handlers.Doctor.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOneDoctorInfoByIdQuery"/> interface implementations
    /// </summary
    public class HandleGetPageOfDoctorInfosQuery : PagedResourcesQueryHandlerBase<Guid, Objects.Doctor, DoctorInfo, IWantPageOfResources<Guid, DoctorInfo>>, IHandleGetPageOfDoctorInfosQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetDoctorInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Doctor"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder">Builder for <see cref="System.Linq.Expressions.Expression{TDelegate}"/>that can map <see cref="Objects.Doctor"/> instances to <see cref="DoctorInfo"/> instances</param>
        public HandleGetPageOfDoctorInfosQuery(IUnitOfWorkFactory factory, ILogger<HandleGetPageOfDoctorInfosQuery> logger, IExpressionBuilder expressionBuilder) : base(factory, expressionBuilder)
        {
        }
    }
}
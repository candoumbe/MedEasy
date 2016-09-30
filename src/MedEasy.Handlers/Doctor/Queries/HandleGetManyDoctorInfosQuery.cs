using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using MedEasy.Validators;
using AutoMapper.QueryableExtensions;
using MedEasy.Queries.Doctor;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Doctor.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOneDoctorInfoByIdQuery"/> interface implementations
    /// </summary
    public class HandleGetManyDoctorInfoQuery : GenericGetManyQueryHandler<Guid, Objects.Doctor, int, DoctorInfo, IWantManyResources<Guid, DoctorInfo>>, IHandleGetManyDoctorInfosQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetDoctorInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Doctor"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder">Builder for <see cref="System.Linq.Expressions.Expression{TDelegate}"/>that can map <see cref="Objects.Doctor"/> instances to <see cref="DoctorInfo"/> instances</param>
        public HandleGetManyDoctorInfoQuery(IUnitOfWorkFactory factory, ILogger<HandleGetManyDoctorInfoQuery> logger, IExpressionBuilder expressionBuilder) : base(logger, factory, expressionBuilder)
        {
        }
    }
}
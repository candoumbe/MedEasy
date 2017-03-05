using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using MedEasy.Validators;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Doctor.Queries;

namespace MedEasy.Handlers.Doctor.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOneDoctorInfoByIdQuery"/> interface implementations
    /// </summary
    public class HandleGetDoctorInfoByIdQuery : GenericGetOneByIdQueryHandler<Guid, Objects.Doctor, Guid, DoctorInfo, IWantOneResource<Guid, Guid, DoctorInfo>, IValidate<IWantOneResource<Guid, Guid, DoctorInfo>>>, IHandleGetDoctorInfoByIdQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetDoctorInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Doctor"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder"></param>
        public HandleGetDoctorInfoByIdQuery(IUnitOfWorkFactory factory, ILogger<HandleGetDoctorInfoByIdQuery> logger, IExpressionBuilder expressionBuilder) : base(Validator<IWantOneResource<Guid, Guid, DoctorInfo>>.Default, logger, factory, expressionBuilder)
        {
        }
    }
}
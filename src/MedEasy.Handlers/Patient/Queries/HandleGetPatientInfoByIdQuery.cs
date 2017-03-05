using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Handlers.Core.Patient.Queries;

namespace MedEasy.Handlers.Patient.Queries
{

    /// <summary>
    /// An instance of this class execute <see cref="IWantOnePatientInfoByIdQuery"/> queries
    /// </summary>
    public class HandleGetPatientInfoByIdQuery : GenericGetOneByIdQueryHandler<Guid, Objects.Patient, Guid, PatientInfo, IWantOneResource<Guid, Guid, PatientInfo>, IValidate<IWantOneResource<Guid, Guid, PatientInfo>>>,  IHandleGetOnePatientInfoByIdQuery
    {
        /// <summary>
        /// Builds a new <see cref="HandleGetPatientInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <param name="validator">Validator to use to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        public HandleGetPatientInfoByIdQuery(IValidate<IWantOneResource<Guid, Guid, PatientInfo>> validator, ILogger<HandleGetPatientInfoByIdQuery> logger, IUnitOfWorkFactory factory,
            IExpressionBuilder expressionBuilder) : base(validator, logger, factory, expressionBuilder)
        {

        }
    }
}

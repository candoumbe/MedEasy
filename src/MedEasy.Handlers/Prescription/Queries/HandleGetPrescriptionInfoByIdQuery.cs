using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Queries;
using MedEasy.Handlers.Queries;

namespace MedEasy.Handlers.Prescription.Queries
{

    /// <summary>
    /// An instance of this class execute <see cref="IWantOnePrescriptionInfoByIdQuery"/> queries
    /// </summary>
    public class HandleGetPrescriptionInfoByIdQuery : GenericGetOneByIdQueryHandler<Guid, Objects.Prescription, int, PrescriptionHeaderInfo, IWantOneResource<Guid, int, PrescriptionHeaderInfo>, IValidate<IWantOneResource<Guid, int, PrescriptionHeaderInfo>>>, IHandleGetOnePrescriptionHeaderQuery
    {
        /// <summary>
        /// Builds a new <see cref="HandleGetPrescriptionInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <param name="validator">Validator to use to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        public HandleGetPrescriptionInfoByIdQuery(IValidate<IWantOneResource<Guid, int, PrescriptionHeaderInfo>> validator, ILogger<HandleGetPrescriptionInfoByIdQuery> logger, IUnitOfWorkFactory factory,
            IExpressionBuilder expressionBuilder) : base(validator, logger, factory, expressionBuilder)
        {

        }
    }
}

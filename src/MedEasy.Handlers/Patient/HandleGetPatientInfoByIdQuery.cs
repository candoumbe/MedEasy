using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;
using MedEasy.Queries;

namespace MedEasy.Handlers.Patient.Queries
{

    /// <summary>
    /// An instance of this class execute <see cref="IGetPatientInfosByIdQuery"/> queries
    /// </summary>
    public class HandleGetPatientDetailsByIdQuery : IHandleGetPatientDetailsByIdQuery
    {
        /// <summary>
        /// Builds a new <see cref="HandleGetPatientDetailsByIdQuery"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <param name="validator">Validator to use to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        public HandleGetPatientDetailsByIdQuery(IValidate<ICreatePatientCommand> validator, ILogger<HandleGetPatientDetailsByIdQuery> logger, IUnitOfWorkFactory factory,
            IExpressionBuilder expressionBuilder)
        {

        }

        public Task<PatientInfo> HandleAsync(IQuery<Guid, int, PatientInfo> query)
        {
            throw new NotImplementedException();
        }
    }
}

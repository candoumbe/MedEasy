using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Prescription;
using System.Threading.Tasks;
using MedEasy.Commands;

namespace MedEasy.Handlers.Prescription.Commands
{

    /// <summary>
    /// An instance of this class process process <see cref="IDeletePrescriptionByIdCommand"/> commands
    /// </summary>
    public class RunDeletePrescriptionByIdCommand : IRunDeletePrescriptionByIdCommand
    {
        /// <summary>
        /// Builds a new <see cref="RunDeletePrescriptionByIdCommand"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <param name="validator">Validator that will be used to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        public RunDeletePrescriptionByIdCommand(IValidate<IDeletePrescriptionByIdCommand> validator, ILogger<RunDeletePrescriptionByIdCommand> logger, IUnitOfWorkFactory factory,
            IExpressionBuilder expressionBuilder) 
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (expressionBuilder == null)
            {
                throw new ArgumentNullException(nameof(expressionBuilder));
            }


        }

        public async Task<Nothing> RunAsync(IDeletePrescriptionByIdCommand command)
        {
            return Nothing.Value;
        }
    }
}

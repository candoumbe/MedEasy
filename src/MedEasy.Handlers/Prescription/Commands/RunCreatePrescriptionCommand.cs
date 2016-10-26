using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Prescription;
using MedEasy.Handlers.Commands;
using System.Threading.Tasks;

namespace MedEasy.Handlers.Prescription.Commands
{

    /// <summary>
    /// An instance of this class process process <see cref="ICreatePrescriptionCommand"/> commands
    /// </summary>
    public class RunCreatePrescriptionCommand : GenericCreateCommandRunner<Guid, Objects.Prescription, CreatePrescriptionInfo, PrescriptionInfo, ICreatePrescriptionCommand>, IRunCreatePrescriptionCommand
    {
        /// <summary>
        /// Builds a new <see cref="RunCreatePrescriptionCommand"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <param name="validator">Validator to use to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        /// <see cref="GenericCreateCommandRunner{TKey, TEntity, TData, TOutput, TCommand}"/>
        public RunCreatePrescriptionCommand(IValidate<ICreatePrescriptionCommand> validator, ILogger<RunCreatePrescriptionCommand> logger, IUnitOfWorkFactory factory,
            IExpressionBuilder expressionBuilder) 
            : base (validator, logger, factory, expressionBuilder)
        {

        }


        
    }
}

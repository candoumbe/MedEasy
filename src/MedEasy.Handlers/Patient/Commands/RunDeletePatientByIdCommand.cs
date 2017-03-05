using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using static MedEasy.Validators.ErrorLevel;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Patient.Commands;
using MedEasy.Handlers.Core.Exceptions;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// An instance of this class process process <see cref="IDeletePatientByIdCommand"/> commands
    /// </summary>
    public class RunDeletePatientByIdCommand : IRunDeletePatientByIdCommand
    {
        private readonly IUnitOfWorkFactory _factory;
        private readonly IValidate<IDeletePatientByIdCommand> _validator;

        /// <summary>
        /// Builds a new <see cref="RunDeletePatientByIdCommand"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <param name="validator">Validator that will be used to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        public RunDeletePatientByIdCommand(IValidate<IDeletePatientByIdCommand> validator, ILogger<RunDeletePatientByIdCommand> logger, IUnitOfWorkFactory factory,
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

            _validator = validator;
            _factory = factory;


        }

        /// <summary>
        /// Deletes a patient by its <see cref="Objects.Patient"/>
        /// </summary>
        /// <param name="command">Comand that wraps id of the <see cref="Objects.Patient"/> to delete.</param>
        public async Task<Nothing> RunAsync(IDeletePatientByIdCommand command)
        {

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            IEnumerable<Task<ErrorInfo>> validationTasks = _validator.Validate(command);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(validationTasks);
            if (errors.Any(x => x.Severity == Error))
            {
                // TODO Log the error if in DEBUG
                throw new CommandNotValidException<Guid>(command.Id, errors);
            }

            using (var uow = _factory.New())
            {
                uow.Repository<Objects.Patient>().Delete(x => x.UUID == command.Data);
                await uow.SaveChangesAsync();

                return Nothing.Value;
            }
        }
    }
}

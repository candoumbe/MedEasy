using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using static MedEasy.Validators.ErrorLevel;
using MedEasy.Commands;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using MedEasy.Handlers.Core.Patient.Commands;
using MedEasy.Handlers.Core.Exceptions;

namespace MedEasy.Handlers.Patient.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchPatientCommand : IRunPatchPatientCommand
    {
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<RunPatchPatientCommand> _logger;
        private IValidate<IPatchCommand<int, Objects.Patient>> _validator;
        
        /// <summary>
        /// Builds a new <see cref="RunPatchPatientCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="validator">Validator for commands that will be run by <see cref="RunAsync(IPatchPatientCommand)"/>.</param>
        public RunPatchPatientCommand(IUnitOfWorkFactory uowFactory, ILogger<RunPatchPatientCommand> logger, IValidate<IPatchCommand<int, Objects.Patient>> validator) 
        {
            if (uowFactory == null)
            {
                throw new ArgumentNullException(nameof(uowFactory));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }


            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            _uowFactory = uowFactory;
            _logger = logger;
            _validator = validator;
        }

        public async Task<Nothing> RunAsync(IPatchCommand<int, Objects.Patient> command)
        {
            _logger.LogInformation($"Start running command : {command}");

            IEnumerable<Task<ErrorInfo>> errorsTasks = _validator.Validate(command);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(errorsTasks);
            if (errors.Any( x => x.Severity == Error))
            {
                _logger.LogInformation($"Command '{command.Id}' is not valid");
                throw new CommandNotValidException<Guid>(command.Id, errors);
            }

            using (var uow = _uowFactory.New())
            {
                JsonPatchDocument<Objects.Patient> changes = command.Data.PatchDocument;
                
                Operation mainDoctorOp =  changes.Operations.SingleOrDefault(x => $"/{nameof(Objects.Patient.MainDoctorId)}".Equals(x.path, StringComparison.OrdinalIgnoreCase));
                if ((mainDoctorOp?.value as int?) != null)
                {
                    int? newDoctorIdValue = (int?)mainDoctorOp.value;
                    if (newDoctorIdValue.HasValue && (!await uow.Repository<Objects.Doctor>().AnyAsync(x => x.Id == newDoctorIdValue.Value)))
                    {
                        throw new NotFoundException($"Doctor '{newDoctorIdValue}' not found");
                    } 
                }

                int patientId = command.Data.Id;
                Objects.Patient source = await uow.Repository<Objects.Patient>()
                    .SingleOrDefaultAsync(x => x.Id == command.Data.Id);

                if (source == null)
                {
                    throw new NotFoundException($"Patient '{patientId}' not found");
                }

                changes.ApplyTo(source);
                await uow.SaveChangesAsync();

                _logger.LogInformation($"Command {command.Id} completed successfully");

                return Nothing.Value;
            }
        }
    }
}

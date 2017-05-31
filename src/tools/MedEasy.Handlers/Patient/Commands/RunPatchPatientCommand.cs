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
using System.Threading;

namespace MedEasy.Handlers.Patient.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchPatientCommand : IRunPatchPatientCommand
    {
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<RunPatchPatientCommand> _logger;
        private IValidate<IPatchCommand<Guid, Objects.Patient>> _validator;
        
        /// <summary>
        /// Builds a new <see cref="RunPatchPatientCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="validator">Validator for commands that will be run by <see cref="RunAsync(IPatchPatientCommand)"/>.</param>
        public RunPatchPatientCommand(IUnitOfWorkFactory uowFactory, ILogger<RunPatchPatientCommand> logger, IValidate<IPatchCommand<Guid, Objects.Patient>> validator) 
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Nothing> RunAsync(IPatchCommand<Guid, Objects.Patient> command, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation($"Start running command : {command}");

            IEnumerable<Task<ErrorInfo>> errorsTasks = _validator.Validate(command);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(errorsTasks);
            if (errors.Any( x => x.Severity == Error))
            {
                _logger.LogInformation($"Command '{command.Id}' is not valid");
                throw new CommandNotValidException<Guid>(command.Id, errors);
            }

            using (IUnitOfWork uow = _uowFactory.New())
            {
                JsonPatchDocument<Objects.Patient> changes = command.Data.PatchDocument;
                
                Operation mainDoctorOp =  changes.Operations.SingleOrDefault(x => $"/{nameof(Objects.Patient.MainDoctorId)}".Equals(x.path, StringComparison.OrdinalIgnoreCase));
                if ((mainDoctorOp?.value is int newDoctorIdValue))
                {
                    if (!await uow.Repository<Objects.Doctor>().AnyAsync(x => x.Id == newDoctorIdValue, cancellationToken))
                    {
                        throw new NotFoundException($"Doctor <{newDoctorIdValue}> not found");
                    } 
                }

                Guid patientId = command.Data.Id;
                Objects.Patient source = await uow.Repository<Objects.Patient>()
                    .SingleOrDefaultAsync(x => x.UUID == command.Data.Id);

                if (source == null)
                {
                    throw new NotFoundException($"Patient <{patientId}> not found");
                }

                changes.ApplyTo(source);
                await uow.SaveChangesAsync();

                _logger.LogInformation($"Command {command.Id} completed successfully");

                return Nothing.Value;
            }
        }
    }
}

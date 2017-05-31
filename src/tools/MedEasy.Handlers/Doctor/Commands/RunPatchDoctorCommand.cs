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
using MedEasy.Handlers.Core.Doctor.Commands;
using MedEasy.Handlers.Core.Exceptions;
using System.Threading;

namespace MedEasy.Handlers.Doctor.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchDoctorCommand : IRunPatchDoctorCommand
    {
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<RunPatchDoctorCommand> _logger;
        private IValidate<IPatchCommand<Guid, Objects.Doctor>> _validator;
        
        /// <summary>
        /// Builds a new <see cref="RunPatchDoctorCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="validator">Validator for commands that will be run by <see cref="RunAsync(IPatchDoctorCommand)"/>.</param>
        public RunPatchDoctorCommand(IUnitOfWorkFactory uowFactory, ILogger<RunPatchDoctorCommand> logger, IValidate<IPatchCommand<Guid, Objects.Doctor>> validator) 
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Nothing> RunAsync(IPatchCommand<Guid, Objects.Doctor> command, CancellationToken cancellationToken = default(CancellationToken))
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
                JsonPatchDocument<Objects.Doctor> changes = command.Data.PatchDocument;
                
                
                Guid doctorId = command.Data.Id;
                Objects.Doctor source = await uow.Repository<Objects.Doctor>()
                    .SingleOrDefaultAsync(x => x.UUID == doctorId);

                if (source == null)
                {
                    throw new NotFoundException($"Doctor '{doctorId}' not found");
                }


                changes.ApplyTo(source);
                await uow.SaveChangesAsync();
                _logger.LogInformation($"Command {command.Id} completed successfully");


                return Nothing.Value;
            }
        }
    }
}

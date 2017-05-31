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
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Handlers.Core.Exceptions;
using System.Threading;

namespace MedEasy.Handlers.Appointment.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchAppointmentCommand : IRunPatchAppointmentCommand
    {
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<RunPatchAppointmentCommand> _logger;
        private IValidate<IPatchCommand<Guid, Objects.Appointment>> _validator;
        
        /// <summary>
        /// Builds a new <see cref="RunPatchAppointmentCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="validator">Validator for commands that will be run by <see cref="RunAsync(IPatchAppointmentCommand)"/>.</param>
        public RunPatchAppointmentCommand(IUnitOfWorkFactory uowFactory, ILogger<RunPatchAppointmentCommand> logger, IValidate<IPatchCommand<Guid, Objects.Appointment>> validator) 
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Nothing> RunAsync(IPatchCommand<Guid, Objects.Appointment> command, CancellationToken cancellationToken = default(CancellationToken))
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
                JsonPatchDocument<Objects.Appointment> changes = command.Data.PatchDocument;
                
                Guid appointmentId = command.Data.Id;
                Objects.Appointment source = await uow.Repository<Objects.Appointment>()
                    .SingleOrDefaultAsync(x => x.UUID == command.Data.Id, cancellationToken);

                if (source == null)
                {
                    throw new NotFoundException($"Appointment <{appointmentId}> not found");
                }


                changes.ApplyTo(source);
                await uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"Command {command.Id} completed successfully");


                return Nothing.Value;
            }
        }
    }
}

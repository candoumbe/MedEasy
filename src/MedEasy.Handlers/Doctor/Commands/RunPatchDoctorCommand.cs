using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using static MedEasy.Validators.ErrorLevel;
using MedEasy.Handlers.Exceptions;
using MedEasy.DTO;
using MedEasy.Commands;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Doctor;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace MedEasy.Handlers.Doctor.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchDoctorCommand : IRunPatchDoctorCommand
    {
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<RunPatchDoctorCommand> _logger;
        private IValidate<IPatchCommand<int, Objects.Doctor>> _validator;
        
        /// <summary>
        /// Builds a new <see cref="RunPatchDoctorCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="validator">Validator for commands that will be run by <see cref="RunAsync(IPatchDoctorCommand)"/>.</param>
        public RunPatchDoctorCommand(IUnitOfWorkFactory uowFactory, ILogger<RunPatchDoctorCommand> logger, IValidate<IPatchCommand<int, Objects.Doctor>> validator) 
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

        public async Task<Nothing> RunAsync(IPatchCommand<int, Objects.Doctor> command)
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
                JsonPatchDocument<Objects.Doctor> changes = command.Data.PatchDocument;
                
                
                int patientId = command.Data.Id;
                Objects.Doctor source = await uow.Repository<Objects.Doctor>()
                    .SingleOrDefaultAsync(x => x.Id == command.Data.Id);

                if (source == null)
                {
                    throw new NotFoundException($"Doctor '{patientId}' not found");
                }


                changes.ApplyTo(source);
                await uow.SaveChangesAsync();
                _logger.LogInformation($"Command {command.Id} completed successfully");


                return Nothing.Value;
            }
        }
    }
}

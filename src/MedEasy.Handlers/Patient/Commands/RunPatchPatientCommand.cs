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

namespace MedEasy.Handlers.Patient.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchPatientCommand : IRunPatchPatientCommand
    {
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<RunPatchPatientCommand> _logger;
        private IValidate<IPatchCommand<int>> _validator;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="RunPatchPatientCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="validator">Validator for commands that will be run by <see cref="RunAsync(IPatchCommand{int})"/>.</param>
        public RunPatchPatientCommand(IUnitOfWorkFactory uowFactory, ILogger<RunPatchPatientCommand> logger, IValidate<IPatchCommand<int>> validator, IExpressionBuilder expressionBuilder)
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

            if (expressionBuilder == null)
            {
                throw new ArgumentNullException(nameof(expressionBuilder));
            }

            _uowFactory = uowFactory;
            _logger = logger;
            _validator = validator;
            _expressionBuilder = expressionBuilder;
        }

        public async Task<PatientInfo> RunAsync(IPatchCommand<int> command)
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
                IEnumerable<ChangeInfo> changes = command.Data.Changes;

                ChangeInfo mainDoctorChange = changes.SingleOrDefault(x => x.Path.Equals($"/{nameof(PatientInfo.MainDoctorId)}", StringComparison.OrdinalIgnoreCase));
                if (mainDoctorChange != null)
                {
                    int? newDoctorId = (int?) mainDoctorChange.Value;
                    if (newDoctorId.HasValue && !await uow.Repository<Objects.Doctor>().AnyAsync(x => x.Id == newDoctorId.Value))
                    {
                        throw new NotFoundException(nameof(PatientInfo.MainDoctorId));
                    }
                }

                Objects.Patient patient = await uow.Repository<Objects.Patient>()
                    .SingleOrDefaultAsync(x => x.Id == command.Data.Id);
                
                if (patient == null)
                {
                    throw new NotFoundException("Patient not found");
                }

                await changes.ForEachAsync(x => Task.Run(() => x.Path.Trim().Replace("/", string.Empty)));

                if(mainDoctorChange != null)
                {
                    patient.MainDoctorId = (int?)mainDoctorChange.Value;
                }

                ChangeInfo firstnameChange = changes.SingleOrDefault(x => x.Path == $"{nameof(PatientInfo.Firstname)}");
                if (firstnameChange != null)
                {
                    patient.Firstname = (string)firstnameChange.Value;
                }

                ChangeInfo lastnameChange = changes.SingleOrDefault(x => x.Path == $"{nameof(PatientInfo.Lastname)}");
                if (firstnameChange != null)
                {
                    patient.Lastname = (string)lastnameChange.Value;
                }

                await uow.SaveChangesAsync();

                Expression<Func<Objects.Patient, PatientInfo>> converter = _expressionBuilder.CreateMapExpression<Objects.Patient, PatientInfo>();

                _logger.LogInformation($"Command {command.Id} completed successfully");

                return converter.Compile()(patient);
            }
        }
    }
}

using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Patient.Commands;

namespace MedEasy.Handlers.Patient.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchPatientCommand : GenericPatchCommandRunner<Guid, Guid, int, Objects.Patient, IPatchCommand<Guid, Objects.Patient>>, IRunPatchPatientCommand
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
            : base(validator, uowFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
    }
}

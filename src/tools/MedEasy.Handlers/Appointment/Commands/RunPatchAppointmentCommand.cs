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
using Optional;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Objects;

namespace MedEasy.Handlers.Appointment.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchAppointmentCommand : GenericPatchCommandRunner<Guid, Guid, int, Objects.Appointment, IPatchCommand<Guid, Objects.Appointment>>, IRunPatchAppointmentCommand
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
        public RunPatchAppointmentCommand(IValidate<IPatchCommand<Guid, Objects.Appointment>> validator, IUnitOfWorkFactory uowFactory) : base(validator, uowFactory)
        {
        }


    }
}

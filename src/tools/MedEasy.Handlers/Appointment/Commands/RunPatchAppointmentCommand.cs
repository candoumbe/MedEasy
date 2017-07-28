using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Handlers.Core.Commands;
using MedEasy.DTO;

namespace MedEasy.Handlers.Appointment.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchAppointmentCommand : GenericPatchCommandRunner<Guid, int, Objects.Appointment>, IRunPatchAppointmentCommand
    {
        
        /// <summary>
        /// Builds a new <see cref="RunPatchAppointmentCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="validator">Validator for commands that will be run by <see cref="RunAsync(IPatchAppointmentCommand)"/>.</param>
        public RunPatchAppointmentCommand(IValidate<IPatchCommand<Guid, Guid,  Objects.Appointment, IPatchInfo<Guid, Objects.Appointment>>> validator, IUnitOfWorkFactory uowFactory) : base(uowFactory)
        {
        }


    }
}

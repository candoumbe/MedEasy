using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Handlers.Core.Commands;
using MedEasy.DTO;
using FluentValidation;

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
        public RunPatchAppointmentCommand(IUnitOfWorkFactory uowFactory) : base(uowFactory)
        {
        }


    }
}

using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Patient.Commands;
using MedEasy.DTO;

namespace MedEasy.Handlers.Patient.Commands
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class RunPatchPatientCommand : GenericPatchCommandRunner<Guid, int, Objects.Patient>, IRunPatchPatientCommand
    {
        /// <summary>
        /// Builds a new <see cref="RunPatchPatientCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="uowFactory"/> is <c>null</c>.</exception>
        public RunPatchPatientCommand(IUnitOfWorkFactory uowFactory)
            : base(uowFactory)
        {
        }
    }
}

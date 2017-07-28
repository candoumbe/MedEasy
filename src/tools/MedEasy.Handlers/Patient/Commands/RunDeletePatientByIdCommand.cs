using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Patient;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Patient.Commands;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// An instance of this class process process <see cref="IDeletePatientByIdCommand"/> commands
    /// </summary>
    public class RunDeletePatientByIdCommand : GenericDeleteByIdCommandRunner<Guid, Objects.Patient, IDeletePatientByIdCommand>, IRunDeletePatientByIdCommand
    {
        /// <summary>
        /// Builds a new <see cref="RunDeletePatientByIdCommand"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <exception cref="ArgumentNullException"> if <paramref name="factory"/> is <c>null</c></exception>
        public RunDeletePatientByIdCommand(IUnitOfWorkFactory factory) : base(factory)
        {
        }
    }
}

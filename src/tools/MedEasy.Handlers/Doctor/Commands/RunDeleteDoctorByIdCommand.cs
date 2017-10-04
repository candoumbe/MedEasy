using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Commands.Doctor;
using MedEasy.Handlers.Core.Doctor.Commands;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;

namespace MedEasy.Handlers.Doctor.Commands
{


    /// <summary>
    /// Processes <see cref="IDeleteDoctorByIdCommand"/> commands
    /// </summary>
    public class RunDeleteDoctorByIdCommand : GenericDeleteByIdCommandRunner<Guid, Objects.Doctor, IDeleteDoctorByIdCommand>, IRunDeleteDoctorInfoByIdCommand
    {
        /// <summary>
        /// Builds a new <see cref="RunDeleteDoctorByIdCommand"/> instance.
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <exception cref="ArgumentNullException"> if <paramref name="factory"/> is <c>null</c>.</exception>
        public RunDeleteDoctorByIdCommand(IUnitOfWorkFactory factory) : base(factory)
        {
        }
    }
}

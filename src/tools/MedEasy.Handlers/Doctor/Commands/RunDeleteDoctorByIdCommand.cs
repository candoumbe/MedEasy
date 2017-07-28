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
    /// An instance of this class process process <see cref="IDeleteDoctorByIdCommand"/> commands
    /// </summary>
    public class RunDeleteDoctorByIdCommand : GenericDeleteByIdCommandRunner<Guid, Objects.Doctor, IDeleteDoctorByIdCommand>, IRunDeleteDoctorInfoByIdCommand
    {
        /// <summary>
        /// Builds a new <see cref="RunDeleteDoctorByIdCommand"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="validator">Validator that will be used to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        public RunDeleteDoctorByIdCommand(IValidate<IDeleteDoctorByIdCommand> validator, ILogger<RunDeleteDoctorByIdCommand> logger, IUnitOfWorkFactory factory) : base(factory)
        {
        }
    }
}

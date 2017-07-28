using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Prescription;
using System.Threading.Tasks;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Prescription.Commands;
using System.Threading;
using MedEasy.Handlers.Core.Commands;

namespace MedEasy.Handlers.Prescription.Commands
{

    /// <summary>
    /// An instance of this class process process <see cref="IDeletePrescriptionByIdCommand"/> commands
    /// </summary>
    public class RunDeletePrescriptionByIdCommand : GenericDeleteByIdCommandRunner<Guid, Objects.Prescription, IDeletePrescriptionByIdCommand>, IRunDeletePrescriptionByIdCommand
    {

        /// <summary>
        /// Builds a new <see cref="RunDeletePrescriptionByIdCommand"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="validator">Validator that will be used to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        public RunDeletePrescriptionByIdCommand(IValidate<IDeletePrescriptionByIdCommand> validator, ILogger<RunDeletePrescriptionByIdCommand> logger, IUnitOfWorkFactory factory) : base(factory)
        {
        }
    }
}

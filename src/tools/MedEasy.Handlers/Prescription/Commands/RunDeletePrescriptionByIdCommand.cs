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
    /// Processes <see cref="IDeletePrescriptionByIdCommand"/> commands.
    /// </summary>
    public class RunDeletePrescriptionByIdCommand : GenericDeleteByIdCommandRunner<Guid, Objects.Prescription, IDeletePrescriptionByIdCommand>, IRunDeletePrescriptionByIdCommand
    {

        /// <summary>
        /// Builds a new <see cref="RunDeletePrescriptionByIdCommand"/> instance.
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <exception cref="ArgumentNullException"> if <paramref name="factory"/>  is <c>null</c></exception>
        public RunDeletePrescriptionByIdCommand(IUnitOfWorkFactory factory) : base(factory)
        {
        }
    }
}

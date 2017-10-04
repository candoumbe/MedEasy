using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Patient.Commands;
using System.Threading;
using MedEasy.Handlers.Core.Exceptions;
using Optional;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// An instance of this class process process <see cref="ICreatePatientCommand"/> commands
    /// </summary>
    public class RunCreatePatientCommand : GenericCreateCommandRunner<Guid, Objects.Patient, CreatePatientInfo, PatientInfo, ICreatePatientCommand>, IRunCreatePatientCommand
    {
        /// <summary>
        /// Builds a new <see cref="RunCreatePatientCommand"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        /// <see cref="GenericCreateCommandRunner{TKey, TEntity, TData, TOutput, TCommand}"/>
        public RunCreatePatientCommand(IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
            : base(factory, expressionBuilder)
        {

        }


        public override async Task<Option<PatientInfo, CommandException>> RunAsync(ICreatePatientCommand command, CancellationToken cancellationToken = default)
        {
            command.Data.Firstname = command.Data.Firstname.ToTitleCase();
            command.Data.Lastname = command.Data.Lastname.ToUpper();

            Option<PatientInfo, CommandException> patientInfo = await base.RunAsync(command, cancellationToken);
            patientInfo.MatchSome(x => x.MainDoctorId = command.Data.MainDoctorId);
            return patientInfo;
        }
    }
}

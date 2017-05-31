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
        /// <param name="validator">Validator to use to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        /// <see cref="GenericCreateCommandRunner{TKey, TEntity, TData, TOutput, TCommand}"/>
        public RunCreatePatientCommand(IValidate<ICreatePatientCommand> validator, ILogger<RunCreatePatientCommand> logger, IUnitOfWorkFactory factory,
            IExpressionBuilder expressionBuilder)
            : base(validator, logger, factory, expressionBuilder)
        {

        }


        public override async Task<PatientInfo> RunAsync(ICreatePatientCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            PatientInfo patientInfo = await base.RunAsync(command, cancellationToken);
            patientInfo.MainDoctorId = command.Data.MainDoctorId;
            return patientInfo;
        }

        public override Task OnCreatingAsync(Guid id, CreatePatientInfo input)
        {
            input.Firstname = input.Firstname?.ToTitleCase();
            input.Lastname = input.Lastname?.ToUpper();

            return base.OnCreatingAsync(id, input);
        }
    }
}

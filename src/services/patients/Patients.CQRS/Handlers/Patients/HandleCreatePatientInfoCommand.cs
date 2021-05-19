namespace Patients.CQRS.Handlers.Patients
{
    using AutoMapper.QueryableExtensions;

    using MedEasy.DAL.Interfaces;

    using MediatR;

    using global::Patients.CQRS.Commands;
    using global::Patients.Events;
    using global::Patients.DTO;
    using global::Patients.Ids;
    using global::Patients.Objects;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using MassTransit;
using MassTransit.Transports;

/// <summary>
/// Handles <see cref="CreatePatientInfoCommand"/>s
/// </summary>
    public class HandleCreatePatientInfoCommand : IRequestHandler<CreatePatientInfoCommand, PatientInfo>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Builds a new <see cref="HandleCreatePatientInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator</param>
        /// <paramref name="mediator"/> is <c>null</c>.
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="expressionBuilder"/> or 
        /// </exception>
        public HandleCreatePatientInfoCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator, IPublishEndpoint publishEndpoint)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }


        public async Task<PatientInfo> Handle(CreatePatientInfoCommand cmd, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            CreatePatientInfo newResourceInfo = cmd.Data;

            Patient entity = new(newResourceInfo.Id ?? PatientId.New(),
                                 firstname: newResourceInfo.Firstname?.ToTitleCase(),
                                 lastname: newResourceInfo.Lastname?.ToUpperInvariant()
            );

            uow.Repository<Patient>().Create(entity);
            await uow.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            Expression<Func<Patient, PatientInfo>> mapEntityToDtoExpression = _expressionBuilder.GetMapExpression<Patient, PatientInfo>();

            PatientInfo patientInfo = mapEntityToDtoExpression.Compile().Invoke(entity);


            await _publishEndpoint.Publish(new PatientCaseCreated(patientInfo.Id, $"{patientInfo.Lastname} {patientInfo.Firstname}", patientInfo.BirthDate), cancellationToken)
                .ConfigureAwait(false);

            return patientInfo;
        }
    }
}

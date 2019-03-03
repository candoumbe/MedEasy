using AutoMapper.QueryableExtensions;
using MedEasy.Abstractions;
using MedEasy.DAL.Interfaces;
using MediatR;
using Patients.CQRS.Commands;
using Patients.CQRS.Events;
using Patients.DTO;
using Patients.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Patients.CQRS.Handlers.Patients
{
    /// <summary>
    /// Handles <see cref="CreatePatientInfoCommand"/>s
    /// </summary>
    public class HandleCreatePatientInfoCommand : IRequestHandler<CreatePatientInfoCommand, PatientInfo>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IMediator _mediator;
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Builds a new <see cref="HandleCreatePatientInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator</param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="expressionBuilder"/> or 
        /// <paramref name="mediator"/> is <c>null</c>.
        /// </exception>
        public HandleCreatePatientInfoCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator, IDateTimeService dateTimeService)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }


        public async Task<PatientInfo> Handle(CreatePatientInfoCommand cmd, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                CreatePatientInfo newResourceInfo = cmd.Data;
                
                Expression<Func<CreatePatientInfo, Patient>> mapDtoToEntityExpression = _expressionBuilder.GetMapExpression<CreatePatientInfo, Patient>();
                Patient entity = mapDtoToEntityExpression.Compile().Invoke(cmd.Data);

                entity.Firstname = cmd.Data.Firstname?.ToTitleCase();
                entity.Lastname = cmd.Data.Lastname.ToUpperInvariant();
                entity.UUID = newResourceInfo.Id.GetValueOrDefault(Guid.NewGuid());
                
                uow.Repository<Patient>().Create(entity);
                await uow.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                Expression<Func<Patient, PatientInfo>> mapEntityToDtoExpression = _expressionBuilder.GetMapExpression<Patient, PatientInfo>();

                PatientInfo patientInfo = mapEntityToDtoExpression.Compile().Invoke(entity);


                await _mediator.Publish(new PatientCreated(patientInfo), cancellationToken)
                    .ConfigureAwait(false);

                return patientInfo;
            }
        }
    }
}

using AutoMapper.QueryableExtensions;
using Measures.CQRS.Commands;
using Measures.CQRS.Events;
using Measures.DTO;
using Measures.Objects;
using MedEasy.DAL.Interfaces;
using MediatR;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers
{
    /// <summary>
    /// Handles <see cref="CreateBloodPressureInfoCommand"/>s
    /// </summary>
    public class HandleCreateBloodPressureInfoCommand : IRequestHandler<CreateBloodPressureInfoCommand, BloodPressureInfo>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IMediator _mediator;

        /// <summary>
        /// Builds a new <see cref="HandleCreateBloodPressureInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory to create <see cref="IUnitOfWork"/>s</param>
        /// <param name="expressionBuilder">Builder to create expressions to map a <see cref="BloodPressure"/> to a <see cref="BloodPressureInfo"/> 
        /// and vice-versa</param>
        /// <param name="mediator">Mediator instance </param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="expressionBuilder"/> or 
        /// <paramref name="mediator"/> is <c>null</c>
        /// </exception>
        public HandleCreateBloodPressureInfoCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        public async Task<BloodPressureInfo> Handle(CreateBloodPressureInfoCommand cmd, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.New())
            {

                CreateBloodPressureInfo data = cmd.Data;
                data.Patient.Firstname = data.Patient?.Firstname?.ToTitleCase() ?? string.Empty;
                data.Patient.Lastname = data.Patient?.Lastname?.ToUpperInvariant() ?? string.Empty;

                Expression<Func<CreateBloodPressureInfo, BloodPressure>> mapBloodPressureInfoToEntity = _expressionBuilder.GetMapExpression<CreateBloodPressureInfo, BloodPressure>();
                Expression<Func<PatientInfo, Patient>> mapPatientInfoToEntity = _expressionBuilder.GetMapExpression<PatientInfo, Patient>();
                BloodPressure newEntity = mapBloodPressureInfoToEntity.Compile().Invoke(data);

                newEntity.Patient.UUID = newEntity.Patient.UUID == default
                    ? Guid.NewGuid()
                    : newEntity.Patient.UUID;

                uow.Repository<BloodPressure>().Create(newEntity);
                await uow.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                Expression<Func<BloodPressure, BloodPressureInfo>> mapEntityToBloodPressureInfo = _expressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                Expression<Func<Patient, PatientInfo>> mapEntityToPatientInfo = _expressionBuilder.GetMapExpression<Patient, PatientInfo>();
                BloodPressureInfo createdResource = mapEntityToBloodPressureInfo.Compile().Invoke(newEntity);
                PatientInfo patientInfo = await uow.Repository<Patient>().SingleAsync(mapEntityToPatientInfo, x => x.UUID == createdResource.PatientId)
                    .ConfigureAwait(false);

                await _mediator.Publish(new BloodPressureCreated(createdResource.Id, createdResource.SystolicPressure, createdResource.DiastolicPressure, createdResource.DateOfMeasure))
                    .ConfigureAwait(false);

                await _mediator.Publish(new PatientCreated(patientInfo.Id, patientInfo.Firstname, patientInfo.Lastname, patientInfo.BirthDate))
                   .ConfigureAwait(false);

                return createdResource;

            }
        }
    }
}

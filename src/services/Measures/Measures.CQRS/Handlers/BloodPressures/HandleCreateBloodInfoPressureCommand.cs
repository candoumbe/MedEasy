using AutoMapper.QueryableExtensions;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Events;
using Measures.CQRS.Events.BloodPressures;
using Measures.DTO;
using Measures.Objects;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers.BloodPressures
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
                Option<Patient> optionalPatient = await uow.Repository<Patient>()
                    .SingleOrDefaultAsync(x => x.UUID == data.Patient.Id)
                    .ConfigureAwait(false);

                Expression<Func<CreateBloodPressureInfo, BloodPressure>> mapBloodPressureInfoToEntity = _expressionBuilder.GetMapExpression<CreateBloodPressureInfo, BloodPressure>();
                BloodPressure newEntity = mapBloodPressureInfoToEntity.Compile().Invoke(data);

                optionalPatient.Match(
                    some: (patient) => newEntity.Patient = patient,
                    none: () =>
                    {
                        Expression<Func<PatientInfo, Patient>> mapPatientInfoToEntity = _expressionBuilder.GetMapExpression<PatientInfo, Patient>();
                        newEntity.Patient = mapPatientInfoToEntity.Compile().Invoke(data.Patient);
                        newEntity.Patient.Firstname = newEntity.Patient.Firstname?.ToTitleCase() ?? string.Empty;
                        newEntity.Patient.Lastname = newEntity.Patient.Lastname?.ToUpperInvariant() ?? string.Empty;

                        newEntity.Patient.UUID = newEntity.Patient.UUID == default
                            ? Guid.NewGuid()
                            : newEntity.Patient.UUID;
                    }
                );

                uow.Repository<BloodPressure>().Create(newEntity);
                await uow.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                Expression<Func<BloodPressure, BloodPressureInfo>> mapEntityToBloodPressureInfo = _expressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                Expression<Func<Patient, PatientInfo>> mapEntityToPatientInfo = _expressionBuilder.GetMapExpression<Patient, PatientInfo>();
                BloodPressureInfo createdResource = mapEntityToBloodPressureInfo.Compile().Invoke(newEntity);
                await _mediator.Publish(new BloodPressureCreated(createdResource.Id, createdResource.SystolicPressure, createdResource.DiastolicPressure, createdResource.DateOfMeasure))
                    .ConfigureAwait(false);

                optionalPatient.MatchNone(async () =>
                {
                    PatientInfo patientInfo = await uow.Repository<Patient>().SingleAsync(mapEntityToPatientInfo, x => x.UUID == createdResource.PatientId)
                        .ConfigureAwait(false);

                    await _mediator.Publish(new PatientCreated(patientInfo.Id, patientInfo.Firstname, patientInfo.Lastname, patientInfo.BirthDate))
                       .ConfigureAwait(false);
                });
                
                return createdResource;

            }
        }
    }
}

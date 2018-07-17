using AutoMapper.QueryableExtensions;
using Measures.CQRS.Commands.Patients;
using Measures.CQRS.Events;
using Measures.DTO;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers.Patients
{
    /// <summary>
    /// Handles <see cref="CreatePatientInfoCommand"/>s
    /// </summary>
    public class HandleCreatePatientInfoCommand : IRequestHandler<CreatePatientInfoCommand, PatientInfo>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IMediator _mediator;

        /// <summary>
        /// Builds a new <see cref="HandleCreatePatientInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator</param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="expressionBuilder"/> or 
        /// <paramref name="mediator"/> is <c>null</c>.
        /// </exception>
        public HandleCreatePatientInfoCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        public async Task<PatientInfo> Handle(CreatePatientInfoCommand cmd, CancellationToken ct)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                NewPatientInfo newResourceInfo = cmd.Data;
                if (newResourceInfo.Id.HasValue && newResourceInfo.Id.Value == default)
                {
                    throw new InvalidOperationException();
                }
                Expression<Func<NewPatientInfo, Patient>> mapDtoToEntityExpression = _expressionBuilder.GetMapExpression<NewPatientInfo, Patient>();
                Patient entity = mapDtoToEntityExpression.Compile().Invoke(cmd.Data);

                entity.Firstname = cmd.Data.Firstname?.ToTitleCase();
                entity.Lastname = cmd.Data.Lastname.ToUpperInvariant();
                entity.UUID = newResourceInfo.Id.GetValueOrDefault(Guid.NewGuid());
                DateTimeOffset now = DateTimeOffset.UtcNow;
                entity.UpdatedDate = now;
                entity.CreatedDate = now;
                
                uow.Repository<Patient>().Create(entity);
                await uow.SaveChangesAsync(ct)
                    .ConfigureAwait(false);

                Expression<Func<Patient, PatientInfo>> mapEntityToDtoExpression = _expressionBuilder.GetMapExpression<Patient, PatientInfo>();

                PatientInfo patientInfo = mapEntityToDtoExpression.Compile().Invoke(entity);


                await _mediator.Publish(new PatientCreated(patientInfo), ct);

                return patientInfo;
            }
        }
    }
}

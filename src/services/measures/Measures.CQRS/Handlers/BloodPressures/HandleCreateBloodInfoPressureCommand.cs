using AutoMapper;

using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Events.BloodPressures;
using Measures.DTO;
using Measures.Objects;

using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;

using MediatR;

using Optional;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers.BloodPressures
{
    /// <summary>
    /// Handles <see cref="CreateBloodPressureInfoForPatientIdCommand"/>s
    /// </summary>
    public class HandleCreateBloodPressureInfoCommand : IRequestHandler<CreateBloodPressureInfoForPatientIdCommand, Option<BloodPressureInfo, CreateCommandResult>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        /// <summary>
        /// Builds a new <see cref="HandleCreateBloodPressureInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory to create <see cref="IUnitOfWork"/>s</param>
        /// <param name="mapper">Builder to create expressions to map a <see cref="BloodPressure"/> to a <see cref="BloodPressureInfo"/> 
        /// and vice-versa</param>
        /// <param name="mediator">Mediator instance </param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="mapper"/> or 
        /// <paramref name="mediator"/> is <c>null</c>
        /// </exception>
        public HandleCreateBloodPressureInfoCommand(IUnitOfWorkFactory uowFactory, IMapper mapper, IMediator mediator)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Option<BloodPressureInfo, CreateCommandResult>> Handle(CreateBloodPressureInfoForPatientIdCommand cmd, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                CreateBloodPressureInfo data = cmd.Data;
                Option<Patient> optionalPatient = await uow.Repository<Patient>()
                    .SingleOrDefaultAsync(x => x.Id == data.PatientId)
                    .ConfigureAwait(false);

                return await optionalPatient.Match(
                    some: async _ =>
                    {
                        BloodPressure newEntity = new BloodPressure(patientId: data.PatientId,
                                                                    id: Guid.NewGuid(),
                                                                    data.DateOfMeasure,
                                                                    data.DiastolicPressure,
                                                                    data.SystolicPressure);

                        uow.Repository<BloodPressure>().Create(newEntity);
                        await uow.SaveChangesAsync(cancellationToken)
                            .ConfigureAwait(false);

                        BloodPressureInfo createdResource = _mapper.Map<BloodPressure, BloodPressureInfo>(newEntity);
                        createdResource.PatientId = data.PatientId;

                        await _mediator.Publish(new BloodPressureCreated(createdResource), cancellationToken)
                            .ConfigureAwait(false);

                        return createdResource.SomeNotNull(CreateCommandResult.Done);
                    },
                    none: () => Task.FromResult(Option.None<BloodPressureInfo, CreateCommandResult>(CreateCommandResult.Failed_NotFound))
                )
                .ConfigureAwait(false);
            }
        }
    }
}

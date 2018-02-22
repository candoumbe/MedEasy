using AutoMapper;
using Measures.CQRS.Events.BloodPressures;
using Measures.DTO;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Optional;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers.BloodPressures
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class HandlePatchBloodPressureInfoCommand : IRequestHandler<PatchCommand<Guid, BloodPressureInfo>, ModifyCommandResult>
    {
        private IUnitOfWorkFactory _uowFactory;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        /// <summary>
        /// Builds a new <see cref="RunPatchAppointmentCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator.</param>
        public HandlePatchBloodPressureInfoCommand(IUnitOfWorkFactory uowFactory, IMapper mapper, IMediator mediator)
        {
            
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<ModifyCommandResult> Handle(PatchCommand<Guid, BloodPressureInfo> command, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.New())
            {
                JsonPatchDocument<BloodPressureInfo> patchDocument = command.Data.PatchDocument;
                
                Guid entityId = command.Data.Id;
                Option<BloodPressure> source = await uow.Repository<BloodPressure>()
                    .SingleOrDefaultAsync(x => x.UUID == command.Data.Id, cancellationToken)
                    .ConfigureAwait(false);

                ModifyCommandResult result = ModifyCommandResult.Failed_NotFound;
                source.MatchSome(async entity =>
                    {
                        JsonPatchDocument<BloodPressure> changes = _mapper.Map<JsonPatchDocument<BloodPressureInfo>, JsonPatchDocument<BloodPressure>>(patchDocument);
                        changes.ApplyTo(entity);
                        await uow.SaveChangesAsync()
                            .ConfigureAwait(false);

                        await _mediator.Publish(new BloodPressureUpdated(entityId), cancellationToken)
                            .ConfigureAwait(false);

                        result = ModifyCommandResult.Done;
                    });

                return result;
            }
        }
    }
}

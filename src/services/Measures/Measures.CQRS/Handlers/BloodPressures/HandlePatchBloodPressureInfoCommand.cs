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
using System.Collections.Generic;
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
        private readonly IDictionary<string, Action<BloodPressure, object>> _actions;

        /// <summary>
        /// Builds a new <see cref="HandlePatchBloodPressureInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator.</param>
        public HandlePatchBloodPressureInfoCommand(IUnitOfWorkFactory uowFactory, IMapper mapper, IMediator mediator)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            //_actions = new Dictionary<string, Action<BloodPressure, object>>
            //{
            //    [$"/{nameof(BloodPressure.SystolicPressure)}"] = (instance, newValue) => instance.ChangeSystolicTo((float)newValue),
            //    [$"/{nameof(BloodPressure.DiastolicPressure)}"] = (instance, newValue) => instance.ChangeDiastolicTo((float)newValue),
            //    [$"/{nameof(BloodPressure.DateOfMeasure)}"] = (instance, newValue) => instance.ChangeDateOfMeasure((DateTime)newValue)
            //};
        }

        public async Task<ModifyCommandResult> Handle(PatchCommand<Guid, BloodPressureInfo> command, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                JsonPatchDocument<BloodPressureInfo> patchDocument = command.Data.PatchDocument;

                Guid entityId = command.Data.Id;
                Option<BloodPressure> source = await uow.Repository<BloodPressure>()
                    .SingleOrDefaultAsync(x => x.Id == command.Data.Id, cancellationToken)
                    .ConfigureAwait(false);

                ModifyCommandResult result = ModifyCommandResult.Failed_NotFound;
                source.MatchSome(async entity =>
                    {
                        JsonPatchDocument<BloodPressure> changes = _mapper.Map<JsonPatchDocument<BloodPressureInfo>, JsonPatchDocument<BloodPressure>>(patchDocument);
                        changes.ApplyTo(entity);
                        await uow.SaveChangesAsync(cancellationToken)
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

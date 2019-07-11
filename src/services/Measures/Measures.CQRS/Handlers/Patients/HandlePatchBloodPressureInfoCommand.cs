using AutoMapper;
using Measures.CQRS.Events;
using Measures.DTO;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers.Patients
{
    /// <summary>
    /// Command runner for <see cref="PatchInfo{TResourceId}"/> commands
    /// </summary>
    public class HandlePatchPatientInfoCommand : IRequestHandler<PatchCommand<Guid, PatientInfo>, ModifyCommandResult>
    {
        private IUnitOfWorkFactory _uowFactory;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        /// <summary>
        /// Builds a new <see cref="HandlePatchPatientInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator.</param>
        public HandlePatchPatientInfoCommand(IUnitOfWorkFactory uowFactory, IMapper mapper, IMediator mediator)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<ModifyCommandResult> Handle(PatchCommand<Guid, PatientInfo> command, CancellationToken ct)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                JsonPatchDocument<PatientInfo> patchDocument = command.Data.PatchDocument;

                Guid entityId = command.Data.Id;
                Option<Patient> source = await uow.Repository<Patient>()
                    .SingleOrDefaultAsync(x => x.Id == command.Data.Id, ct)
                    .ConfigureAwait(false);

                ModifyCommandResult result = ModifyCommandResult.Failed_NotFound;
                source.MatchSome(async entity =>
                    {
                        JsonPatchDocument<Patient> changes = _mapper.Map<JsonPatchDocument<PatientInfo>, JsonPatchDocument<Patient>>(patchDocument);
                        if (changes.Operations.Once(op => op.OperationType !=  OperationType.Test && op.path == $"/{nameof(Patient.Name)}"))
                        {
                            entity.ChangeNameTo(changes.Operations.Single(op => op.path == $"/{nameof(Patient.Name)}").value?.ToString());
                        }
                        await uow.SaveChangesAsync(ct)
                            .ConfigureAwait(false);

                        await _mediator.Publish(new PatientUpdated(entityId), ct)
                            .ConfigureAwait(false);

                        result = ModifyCommandResult.Done;
                    });

                return result;
            }
        }
    }
}

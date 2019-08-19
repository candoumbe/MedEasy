using AutoMapper.QueryableExtensions;
using Measures.CQRS.Commands.Patients;
using Measures.CQRS.Events;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers.Patients
{
    /// <summary>
    /// Handles <see cref="DeletePatientInfoByIdCommand"/>s
    /// </summary>
    public class HandleDeletePatientInfoByIdCommand : IRequestHandler<DeletePatientInfoByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IMediator _mediator;

        /// <summary>
        /// Builds a new <see cref="HandleDeletePatientInfoByIdCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator</param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="expressionBuilder"/> or 
        /// <paramref name="mediator"/> is <c>null</c>.
        /// </exception>
        public HandleDeletePatientInfoByIdCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<DeleteCommandResult> Handle(DeletePatientInfoByIdCommand cmd, CancellationToken cancellationToken)
        {
            IEnumerable<Task<bool>> anyMesureTask = new[]{
                    _uowFactory.NewUnitOfWork().Repository<BloodPressure>().AnyAsync(x => x.Patient.Id == cmd.Data, cancellationToken).AsTask(),
                    _uowFactory.NewUnitOfWork().Repository<Temperature>().AnyAsync(x => x.Patient.Id == cmd.Data, cancellationToken).AsTask(),
            };

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                DeleteCommandResult result = DeleteCommandResult.Done;

                await Task.WhenAll(anyMesureTask).ConfigureAwait(false);

                if (anyMesureTask.AtLeastOnce(anyMesure => anyMesure.Result))
                {
                    result = DeleteCommandResult.Failed_Conflict;
                }
                else if (!await uow.Repository<Patient>()
                    .AnyAsync(x => x.Id == cmd.Data, cancellationToken).ConfigureAwait(false))
                {
                    result = DeleteCommandResult.Failed_NotFound;
                }
                else
                {
                    uow.Repository<Patient>().Delete(x => x.Id == cmd.Data);
                    await uow.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);

                    await _mediator.Publish(new PatientDeleted(cmd.Data), cancellationToken)
                        .ConfigureAwait(false);
                }

                return result;
            }
        }
    }
}

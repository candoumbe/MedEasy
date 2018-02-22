using AutoMapper.QueryableExtensions;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Events.BloodPressures;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers.BloodPressures
{
    /// <summary>
    /// Handles <see cref="DeleteBloodPressureInfoByIdCommand"/>s
    /// </summary>
    public class HandleDeleteBloodPressureInfoByIdCommand : IRequestHandler<DeleteBloodPressureInfoByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IMediator _mediator;

        /// <summary>
        /// Builds a new <see cref="HandleCreateBloodPressureInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator</param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="expressionBuilder"/> or 
        /// <paramref name="mediator"/> is <c>null</c>.
        /// </exception>
        public HandleDeleteBloodPressureInfoByIdCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        public async Task<DeleteCommandResult> Handle(DeleteBloodPressureInfoByIdCommand cmd, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.New())
            {
                DeleteCommandResult result = DeleteCommandResult.Done;
                if (await uow.Repository<BloodPressure>().AnyAsync(x => x.UUID == cmd.Data).ConfigureAwait(false))
                {
                    uow.Repository<BloodPressure>().Delete(x => x.UUID == cmd.Data);
                    await uow.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);

                    await _mediator.Publish(new BloodPressureDeleted(cmd.Data), cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    result = DeleteCommandResult.Failed_NotFound;
                }

                return result;
            }
        }
    }
}

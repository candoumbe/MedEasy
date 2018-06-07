using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Identity.CQRS.Commands.Accounts;
using Identity.CQRS.Events.Accounts;
using Identity.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;

namespace Identity.CQRS.Handlers.Commands.Accounts
{
    /// <summary>
    /// Handles <see cref="DeleteAccountByIdCommand"/>s
    /// </summary>
    public class HandleDeleteAccountInfoByIdCommand : IRequestHandler<DeleteAccountInfoByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IMediator _mediator;

        /// <summary>
        /// Builds a new <see cref="HandleDeleteAccountInfoByIdCommand"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="mediator">mediator service used to send notifications</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="unitOfWorkFactory"/> or <paramref name="mediator"/> is <c>null</c>;</exception>
        public HandleDeleteAccountInfoByIdCommand(IUnitOfWorkFactory unitOfWorkFactory, IMediator mediator)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<DeleteCommandResult> Handle(DeleteAccountInfoByIdCommand cmd, CancellationToken ct)
        {
            Guid idToDelete = cmd.Data;

            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                DeleteCommandResult cmdResult = DeleteCommandResult.Failed_NotFound;
                if(await uow.Repository<Account>().AnyAsync(x => x.UUID == idToDelete, ct).ConfigureAwait(false))
                {
                    uow.Repository<Account>().Delete(x => x.UUID == idToDelete);
                    await uow.SaveChangesAsync(ct)
                        .ConfigureAwait(false);

                    await _mediator.Publish(new AccountDeleted(idToDelete), ct)
                        .ConfigureAwait(false);
                    cmdResult = DeleteCommandResult.Done;
                }

                return cmdResult;
                
            }
        }
    }
}

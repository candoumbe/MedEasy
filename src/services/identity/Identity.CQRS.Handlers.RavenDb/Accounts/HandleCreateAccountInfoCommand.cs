using AutoMapper;
using Identity.CQRS.Commands.Accounts;
using Identity.CQRS.Events.Accounts;
using Identity.CQRS.Queries;
using Identity.DTO;
using Identity.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MediatR;
using Optional;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.RavenDb.Accounts
{
    public class HandleCreateAccountInfoCommand : IRequestHandler<CreateAccountInfoCommand, Option<AccountInfo, CreateCommandResult>>
    {
        private readonly IDocumentStore _store;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public HandleCreateAccountInfoCommand(IDocumentStore store, IMediator mediator, IMapper mapper)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Option<AccountInfo, CreateCommandResult>> Handle(CreateAccountInfoCommand request, CancellationToken cancellationToken)
        {
            using (IAsyncDocumentSession session = _store.OpenAsyncSession())
            {
                Option<AccountInfo, CreateCommandResult> response;
                if (await session.Query<Account>().AnyAsync(acc => acc.Username == request.Data.Username).ConfigureAwait(false))
                {
                    response = Option.None<AccountInfo, CreateCommandResult>(CreateCommandResult.Failed_Conflict);
                }
                else
                {
                    (string salt, string passwordHash) = await _mediator.Send(new HashPasswordQuery(request.Data.Password), cancellationToken)
                            .ConfigureAwait(false);

                    Account newAccount = new Account(
                        uuid: Guid.NewGuid(),
                        username: request.Data.Username,
                        name: request.Data.Name ?? request.Data.Username,
                        passwordHash: passwordHash,
                        salt: salt,
                        email: request.Data.Email,
                        locked: false,
                        isActive: false,
                        tenantId: request.Data.TenantId);

                    await session.StoreAsync(newAccount, cancellationToken)
                        .ConfigureAwait(false);

                    await session.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);

                    AccountInfo createdAccountInfo = _mapper.Map<AccountInfo>(newAccount);

                    await _mediator.Publish(new AccountCreated(createdAccountInfo))
                        .ConfigureAwait(false); 

                    response = Option.Some<AccountInfo, CreateCommandResult>(createdAccountInfo);
                }

                return response;
            }
        }
    }
}

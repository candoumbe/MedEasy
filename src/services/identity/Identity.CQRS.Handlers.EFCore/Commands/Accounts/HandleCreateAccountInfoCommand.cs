using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Identity.CQRS.Commands.Accounts;
using Identity.CQRS.Queries;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using Identity.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;

namespace Identity.CQRS.Handlers.EFCore.Commands.Accounts
{
    /// <summary>
    /// Handles creation of <see cref="AccountInfo"/>s
    /// </summary>
    public class HandleCreateAccountInfoCommand : IRequestHandler<CreateAccountInfoCommand, Option<AccountInfo, CreateCommandResult>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        /// <summary>
        /// Creates a new <see cref="HandleCreateAccountInfoCommand"/> instance
        /// </summary>
        /// <param name="uowFactory">Factory for creating <see cref="IUnitOfWork"/>s</param>
        /// <param name="mapper">Service to map one type of instance to another</param>
        /// <param name="mediator"></param>
        public HandleCreateAccountInfoCommand(IUnitOfWorkFactory uowFactory, IMapper mapper, IMediator mediator)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Option<AccountInfo, CreateCommandResult>> Handle(CreateAccountInfoCommand request, CancellationToken ct)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            Option<AccountInfo, CreateCommandResult> cmdResult = Option.None<AccountInfo, CreateCommandResult>(CreateCommandResult.Failed_Conflict);
            NewAccountInfo data = request.Data;
            if (data.Password == data.ConfirmPassword && !await uow.Repository<Account>().AnyAsync(x => x.Username == data.Username, ct).ConfigureAwait(false))
            {
                (string salt, string passwordHash) = await _mediator.Send(new HashPasswordQuery(data.Password), ct)
                                                                    .ConfigureAwait(false);

                Account newEntity = _mapper.Map<NewAccountInfo, Account>(data);
                newEntity.SetPassword(passwordHash, salt);

                uow.Repository<Account>().Create(newEntity);

                await uow.SaveChangesAsync(ct)
                    .ConfigureAwait(false);

                Option<AccountInfo> optionalAccountInfo = await _mediator.Send(new GetOneAccountByIdQuery(newEntity.Id), ct)
                    .ConfigureAwait(false);

                optionalAccountInfo.MatchSome(newAccountInfo => cmdResult = Option.Some<AccountInfo, CreateCommandResult>(newAccountInfo));
            }

            return cmdResult;
        }
    }
}

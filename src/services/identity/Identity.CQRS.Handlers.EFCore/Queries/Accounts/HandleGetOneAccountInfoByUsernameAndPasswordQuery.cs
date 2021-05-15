namespace Identity.CQRS.Handlers.Queries.Accounts
{
    using AutoMapper.QueryableExtensions;

    using Identity.CQRS.Queries;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DTO;
    using Identity.Objects;

    using MedEasy.DAL.Interfaces;

    using MediatR;

    using Optional;

    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class HandleGetOneAccountInfoByUsernameAndPasswordQuery : IRequestHandler<GetOneAccountByUsernameAndPasswordQuery, Option<AccountInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IMediator _mediator;

        public HandleGetOneAccountInfoByUsernameAndPasswordQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
            _mediator = mediator;
        }

        public async Task<Option<AccountInfo>> Handle(GetOneAccountByUsernameAndPasswordQuery request, CancellationToken ct)
        {
            LoginInfo data = request.Data;

            IUnitOfWork uow = _uowFactory.NewUnitOfWork();

            Expression<Func<RoleClaim, ClaimInfo>> roleClaimToClaimInfoSelector = _expressionBuilder.GetMapExpression<RoleClaim, ClaimInfo>();
            Expression<Func<AccountClaim, ClaimInfo>> userClaimToClaimInfoSelector = _expressionBuilder.GetMapExpression<AccountClaim, ClaimInfo>();

            var usernameAndPassword = await uow.Repository<Account>()
                                                .SingleOrDefaultAsync(x => new
                                                {
                                                    x.Id,
                                                    x.Name,
                                                    x.Username,
                                                    x.Email,
                                                    x.PasswordHash,
                                                    x.Salt
                                                },
                                                                      (Account x) => x.Username == data.Username,
                                                                      ct
                                                )
                                                .ConfigureAwait(false);

            return await usernameAndPassword.Match(
                some: async userFoundData =>
                {
                    string currentPassword = await _mediator.Send(new HashPasswordWithPredefinedSaltAndIterationQuery((request.Data.Password, userFoundData.Salt, 10_000)), ct)
                                                            .ConfigureAwait(false);

                    Option<AccountInfo> accountInfo = Option.None<AccountInfo>();
                    if (currentPassword == userFoundData.PasswordHash)
                    {
                        var account = await uow.Repository<Account>()
                                                    .SingleAsync(
                                                        x => new
                                                        {
                                                            x.Id,
                                                            x.Name,
                                                            x.Username,
                                                            x.Email,
                                                            Roles = x.Roles
                                                                .Select(ar => new RoleInfo { Name = ar.Role.Code, Claims = ar.Role.Claims.Select(rc => new ClaimInfo { Type = rc.Claim.Type, Value = rc.Claim.Value }) })
                                                                .ToList(),
                                                            Claims = x.Claims.Select(rc => new ClaimInfo { Type = rc.Claim.Type, Value = rc.Claim.Value })
                                                        },
                                                        (Account x) => x.Id == userFoundData.Id,
                                                        ct
                                                    )
                                                    .ConfigureAwait(false);

                        accountInfo = Option.Some(new AccountInfo
                        {
                            Id = account.Id,
                            Name = account.Name ?? account.Username,
                            Username = account.Username,
                            Email = account.Email,
                            Roles = account.Roles,
                            Claims = account.Claims
                        });
                    }
                    return accountInfo;
                },
                none: () => Task.FromResult(Option.None<AccountInfo>())
            )
                .ConfigureAwait(false);
        }
    }
}

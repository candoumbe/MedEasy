using AutoMapper.QueryableExtensions;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using Identity.Objects;
using MedEasy.Abstractions;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Queries.Accounts
{
    public class HandleGetOneAccountInfoByUsernameAndPasswordQuery : IRequestHandler<GetOneAccountByUsernameAndPasswordQuery, Option<AccountInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IDateTimeService _dateTimeService;

        public HandleGetOneAccountInfoByUsernameAndPasswordQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IDateTimeService dateTimeService)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
            _dateTimeService = dateTimeService;
        }

        public async Task<Option<AccountInfo>> Handle(GetOneAccountByUsernameAndPasswordQuery request, CancellationToken ct)
        {
            LoginInfo data = request.Data;
            DateTimeOffset now = _dateTimeService.UtcNowOffset();
            IUnitOfWork uow = _uowFactory.NewUnitOfWork();

            Expression<Func<RoleClaim, ClaimInfo>> roleClaimToClaimInfoSelector = _expressionBuilder.GetMapExpression<RoleClaim, ClaimInfo>();
            Expression<Func<AccountClaim, ClaimInfo>> userClaimToClaimInfoSelector = _expressionBuilder.GetMapExpression<AccountClaim, ClaimInfo>();
            var optionalUser = await uow.Repository<Account>()
                .SingleOrDefaultAsync(
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
                    (Account x) => x.Username == data.Username && x.PasswordHash == data.Password,
                    ct
                )
                .ConfigureAwait(false);


            return optionalUser.Match(
                some: account =>
                {
                    AccountInfo accountInfo = new AccountInfo
                    {
                        Id = account.Id,
                        Name = account.Name ?? account.Username,
                        Username = account.Username,
                        Email = account.Email,
                        Roles = account.Roles,
                        Claims = account.Claims
                    };
                    return Option.Some(accountInfo);
                },
                none: () => Option.None<AccountInfo>()
            );

        }
    }
}

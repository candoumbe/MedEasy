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
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {

                Expression<Func<RoleClaim, ClaimInfo>> roleClaimToClaimInfoSelector = _expressionBuilder.GetMapExpression<RoleClaim, ClaimInfo>();
                Expression<Func<AccountClaim, ClaimInfo>> userClaimToClaimInfoSelector = _expressionBuilder.GetMapExpression<AccountClaim, ClaimInfo>();
                var optionalUser = await uow.Repository<Account>()
                    .SingleOrDefaultAsync(
                        x => new
                        {
                            x.UserName,
                            x.Email,
                            Roles = x.Roles
                                .Select(r => r.Id),
                            AccountClaims = x.Claims
                                .Where(uc => uc.Start <= now && (uc.End == null || now <= uc.End))
                                .Select(uc => new ClaimInfo { Type = uc.Claim.Type, Value = uc.Claim.Value })
                        },
                        (Account x) => x.UserName == data.Username && x.PasswordHash == data.Password,
                        ct
                    )
                    .ConfigureAwait(false);


                return await optionalUser.Match(
                    some: async account =>
                    {
                        IEnumerable<ClaimInfo> claimsFromRoles = await uow.Repository<RoleClaim>()
                            .WhereAsync(
                                roleClaimToClaimInfoSelector,
                                (RoleClaim rc) => account.Roles.Any(roleId => roleId == rc.RoleId) && !account.AccountClaims.Any(uc => uc.Type == rc.Claim.Type),
                                ct
                            )
                            .ConfigureAwait(false);

                        AccountInfo accountInfo = new AccountInfo
                        {
                            Username = account.UserName,
                            Email = account.Email,
                            Claims = account
                                .AccountClaims
                                .Union(claimsFromRoles)
                        };
                        return Option.Some(accountInfo);
                    },
                    none: () => Task.FromResult(Option.None<AccountInfo>())
                );
            }

        }
    }
}

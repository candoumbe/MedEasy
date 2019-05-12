using AutoMapper.QueryableExtensions;
using DataFilters;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using Identity.Objects;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Queries.Accounts
{
    /// <summary>
    /// Handles <see cref="GetPageOfAccountsByTenantIdQuery"/> queries
    /// </summary>
    public class HandleGetPageOfAccountByTenantIdQuery : 
        IRequestHandler<GetPageOfAccountsByTenantIdQuery, Page<AccountInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetPageOfAccountByTenantIdQuery"/> instance
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfAccountByTenantIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }

        public async Task<Page<AccountInfo>> Handle(GetPageOfAccountsByTenantIdQuery request, CancellationToken ct)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                GetPageOfAccountInfoByTenantIdInfo data = request.Data;
                Expression<Func<Account, AccountInfo>> selector = _expressionBuilder.GetMapExpression<Account,AccountInfo>();
                Page<Account> result = await uow.Repository<Account>()
                    .WhereAsync(
                        //selector: selector,
                        predicate : (Account account) => account.TenantId == data.TenantId,
                        orderBy: new Sort<Account>(nameof(Account.UpdatedDate), SortDirection.Descending),
                        pageSize: data.PageSize,
                        page: data.Page,
                        ct)
                    .ConfigureAwait(false);

                return await new ValueTask<Page<AccountInfo>>(new Page<AccountInfo>(result.Entries.Select(selector.Compile()), result.Total, result.Size));
            }
        }
    }
}

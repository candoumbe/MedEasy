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
                Page<AccountInfo> result = await uow.Repository<Account>()
                    .ReadPageAsync(
                        selector: selector,
                        pageSize: data.PageSize,
                        page: data.Page,
                        orderBy: new Sort<AccountInfo>(nameof(AccountInfo.UpdatedDate), SortDirection.Descending).ToOrderClause(),
                        ct)
                    .ConfigureAwait(false);

                return await new ValueTask<Page<AccountInfo>>(result.Entries.Any() ? result : Page<AccountInfo>.Empty);
            }
        }
    }
}

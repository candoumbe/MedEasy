namespace Identity.CQRS.Handlers.Queries.Accounts
{
    using AutoMapper.QueryableExtensions;

    using DataFilters;

    using Identity.CQRS.Queries.Accounts;
    using Identity.DTO;
    using Identity.Objects;

    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.RestObjects;

    using MediatR;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="GetPageOfAccountQuery"/> queries
    /// </summary>
    public class HandleGetPageOfAccountsQuery :
        IRequestHandler<GetPageOfAccountsQuery, Page<AccountInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetPageOfAccountByTenantIdQuery"/> instance
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfAccountsQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }

        public Task<Page<AccountInfo>> Handle(GetPageOfAccountsQuery request, CancellationToken ct)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

            PaginationConfiguration data = request.Data;
            Expression<Func<Account, AccountInfo>> selector = _expressionBuilder.GetMapExpression<Account, AccountInfo>();

            return uow.Repository<Account>()
                      .ReadPageAsync(
                            selector: selector,
                            pageSize: data.PageSize,
                            page: data.Page,
                            orderBy: new Sort<AccountInfo>(nameof(AccountInfo.UpdatedDate), SortDirection.Descending),
                            ct: ct)
                      .AsTask();
        }
    }
}

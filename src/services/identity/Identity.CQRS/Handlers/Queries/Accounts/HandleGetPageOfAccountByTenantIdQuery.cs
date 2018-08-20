﻿using AutoMapper.QueryableExtensions;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using Identity.Objects;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MedEasy.DAL.Repositories.SortDirection;

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
                Page<AccountInfo> result = await uow.Repository<Account>()
                    .WhereAsync(
                        selector: x => new AccountInfo {
                            Id = x.UUID,
                            TenantId = x.TenantId,
                            Email = x.Email,
                            Name = x.Name,
                            Username = x.UserName,
                        },
                        predicate: (Account x) => x.TenantId == data.TenantId,
                        orderBy: new[] {
                            OrderClause<AccountInfo>.Create(x => x.Id, Descending)
                        },
                        pageSize: data.PageSize,
                        page: data.Page,
                        ct)
                    .ConfigureAwait(false);

                return await new ValueTask<Page<AccountInfo>>(result.Entries.Any() ? result : Page<AccountInfo>.Default);
            }
        }
    }
}
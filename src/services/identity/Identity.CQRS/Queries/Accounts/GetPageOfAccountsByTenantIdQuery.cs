using Identity.DTO;

using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;

using System;

namespace Identity.CQRS.Queries.Accounts
{
    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="AccountInfo"/>s.
    /// </summary>
    public class GetPageOfAccountsByTenantIdQuery : IQuery<Guid, GetPageOfAccountInfoByTenantIdInfo, Page<AccountInfo>>
    {
        public Guid Id { get; }

        public GetPageOfAccountInfoByTenantIdInfo Data { get; }

        /// <summary>
        /// Builds a new <see cref="GetPageOfAccountsQuery"/> instance.
        /// </summary>
        /// <param name="pagination">The paging configuration</param>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c>.</exception>
        public GetPageOfAccountsByTenantIdQuery(GetPageOfAccountInfoByTenantIdInfo data)
        {
            Id = Guid.NewGuid();
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}
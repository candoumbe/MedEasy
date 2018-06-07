using Identity.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.RestObjects;
using System;

namespace Identity.CQRS.Queries.Accounts
{
    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="AccountInfo"/>s.
    /// </summary>
    public class GetPageOfAccountInfoQuery : GetPageOfResourcesQuery<Guid, AccountInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfAccountInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination">The paging configuration</param>
        public GetPageOfAccountInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}
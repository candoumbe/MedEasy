namespace Identity.CQRS.Queries.Accounts
{
    using Identity.DTO;

    using MedEasy.CQRS.Core.Queries;
    using MedEasy.RestObjects;

    using System;

    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="AccountInfo"/>s.
    /// </summary>
    public class GetPageOfAccountsQuery : GetPageOfResourcesQuery<Guid, AccountInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfAccountsQuery"/> instance.
        /// </summary>
        /// <param name="pagination">The paging configuration</param>
        public GetPageOfAccountsQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}
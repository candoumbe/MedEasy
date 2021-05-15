namespace Identity.CQRS.Queries.Accounts
{
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.Ids;

    using System;

    /// <summary>
    /// Query to check if a <see cref="Guid"/> stands for an "tenant"
    /// </summary>
    public class IsTenantQuery : QueryBase<Guid, TenantId, bool>
    {
        /// <summary>
        /// Builds a new <see cref="IsTenantQuery"/> instance
        /// </summary>
        /// <param name="potentialTenantId">id of an <see cref="DTO.AccountInfo"/> that may be a tenant</param>
        public IsTenantQuery(TenantId potentialTenantId) : base(Guid.NewGuid(), potentialTenantId)
        {
        }
    }
}

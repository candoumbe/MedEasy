namespace Identity.CQRS.Queries.Roles
{
    using Identity.DTO;
    using Identity.Ids;

    using MedEasy.CQRS.Core.Queries;

    using Optional;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Query to list all roles an account is attached to.
    /// </summary>
    public class ListRolesForAccountQuery : QueryBase<Guid, AccountId, Option<IEnumerable<RoleInfo>>>
    {
        /// <summary>
        /// Builds a new <see cref="ListRolesForAccountQuery"/> instance.
        /// </summary>
        /// <param name="accountId">identifier of the account which the query is performed for.</param>
        public ListRolesForAccountQuery(AccountId accountId) : base(Guid.NewGuid(), accountId)
        {
        }
    }
}

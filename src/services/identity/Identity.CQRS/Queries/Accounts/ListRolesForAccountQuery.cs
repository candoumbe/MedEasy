using Identity.DTO;

using MedEasy.CQRS.Core.Queries;

using Optional;

using System;
using System.Collections.Generic;

namespace Identity.CQRS.Queries.Roles
{
    /// <summary>
    /// Query to list all roles an account is attached to.
    /// </summary>
    public class ListRolesForAccountQuery : QueryBase<Guid, Guid, Option<IEnumerable<RoleInfo>>>
    {
        /// <summary>
        /// Builds a new <see cref="ListRolesForAccountQuery"/> instance.
        /// </summary>
        /// <param name="accountId">identifier of the account which the query is performed for.</param>
        public ListRolesForAccountQuery(Guid accountId) : base(Guid.NewGuid(), accountId)
        {
        }
    }
}

using Identity.DTO;
using Identity.Ids;

using MedEasy.CQRS.Core.Queries;

using Optional;

using System;
using System.Collections.Generic;

namespace Identity.CQRS.Queries.Roles
{
    /// <summary>
    /// Query to list all accounts that have a specified <see cref="RoleInfo"/>.
    /// </summary>
    public class ListAccountsForRoleQuery : QueryBase<Guid, RoleId, Option<IEnumerable<AccountInfo>>>
    {
        /// <summary>
        /// Builds a new <see cref="ListAccountsForRoleQuery"/> instance.
        /// </summary>
        /// <param name="roleId">identifier of the account which the query is performed for.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="roleId"/> is <see cref="Guid.Empty"/>.</exception>
        public ListAccountsForRoleQuery(RoleId roleId) : base(Guid.NewGuid(), roleId)
        {
        }
    }
}

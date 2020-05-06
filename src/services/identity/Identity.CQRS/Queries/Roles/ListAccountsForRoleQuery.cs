using Identity.DTO;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.CQRS.Queries.Roles
{
    /// <summary>
    /// Query to list all accounts that have a specified <see cref="RoleInfo"/>.
    /// </summary>
    public class ListAccountsForRoleQuery : QueryBase<Guid, Guid, Option<IEnumerable<AccountInfo>>>
    {
        /// <summary>
        /// Builds a new <see cref="ListAccountsForRoleQuery"/> instance.
        /// </summary>
        /// <param name="roleId">identifier of the account which the query is performed for.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="roleId"/> is <see cref="Guid.Empty"/>.</exception>
        public ListAccountsForRoleQuery(Guid roleId) : base(Guid.NewGuid(), roleId)
        {
        }
    }
}

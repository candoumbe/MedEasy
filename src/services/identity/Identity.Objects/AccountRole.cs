
using System;

namespace Identity.Objects
{
    /// <summary>
    /// Relation between a <see cref="Role"/> and an <see cref="Account"/>.
    /// </summary>
    public class AccountRole
    {
        public Guid AccountId { get; }

        public Guid RoleId { get; }

        public Role Role { get; set; }

        public Account Account { get; }

        public AccountRole(Guid accountId, Guid roleId)
        {
            AccountId = accountId;
            RoleId = roleId;
        }
    }
}

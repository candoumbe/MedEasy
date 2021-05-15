namespace Identity.Objects
{

    using Identity.Ids;

    /// <summary>
    /// Relation between a <see cref="Role"/> and an <see cref="Account"/>.
    /// </summary>
    public class AccountRole
    {
        public AccountId AccountId { get; }

        public RoleId RoleId { get; }

        public Role Role { get; set; }

        public Account Account { get; }

        public AccountRole(AccountId accountId, RoleId roleId)
        {
            AccountId = accountId;
            RoleId = roleId;
        }
    }
}

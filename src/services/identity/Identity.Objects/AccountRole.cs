namespace Identity.Objects
{
    using Identity.Ids;

    /// <summary>
    /// Relation between a <see cref="Role"/> and an <see cref="Account"/>.
    /// </summary>
    public class AccountRole
    {
        /// <summary>
        /// Id of the <see cref="Account"/>
        /// </summary>
        public AccountId AccountId { get; }

        /// <summary>
        /// Identifies the <see cref="Role"/> of the association
        /// </summary>
        public RoleId RoleId { get; }

        /// <summary>
        /// <see cref="Role"/> of the association
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// Account of the association
        /// </summary>
        public Account Account { get; }

        /// <summary>
        /// Builds a new <see cref="AccountRole"/> instance
        /// </summary>
        /// <param name="accountId">Identifier of the account</param>
        /// <param name="roleId">Identifier of the role</param>
        public AccountRole(AccountId accountId, RoleId roleId)
        {
            AccountId = accountId;
            RoleId = roleId;
        }
    }
}

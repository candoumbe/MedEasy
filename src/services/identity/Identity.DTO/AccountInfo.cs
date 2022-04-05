namespace Identity.DTO
{
    using Identity.Ids;
    using Identity.ValueObjects;

    using MedEasy.Ids;
    using MedEasy.RestObjects;

    using System.Collections.Generic;

    public class AccountInfo : Resource<AccountId>
    {
        public UserName Username { get; set; }

        public Email Email { get; set; }

        public bool Locked { get; set; }

        /// <summary>
        /// Name associated with the account
        /// </summary>
        public string Name { get; set; }

        public TenantId TenantId { get; set; }

        public IEnumerable<ClaimInfo> Claims { get; set; }

        public IEnumerable<RoleInfo> Roles { get; set; }

        public AccountInfo() => Claims = new List<ClaimInfo>();
    }
}

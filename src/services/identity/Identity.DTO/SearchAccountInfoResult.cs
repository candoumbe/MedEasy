using Identity.Ids;

using MedEasy.Ids;
using MedEasy.RestObjects;

namespace Identity.DTO
{
    public class SearchAccountInfoResult : Resource<AccountId>
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public bool Locked { get; set; }

        /// <summary>
        /// Name associated with the account
        /// </summary>
        public string Name { get; set; }

        public TenantId? TenantId { get; set; }
    }
}

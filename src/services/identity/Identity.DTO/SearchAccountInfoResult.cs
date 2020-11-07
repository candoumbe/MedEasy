using MedEasy.RestObjects;

using System;

namespace Identity.DTO
{
    public class SearchAccountInfoResult : Resource<Guid>
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public bool Locked { get; set; }

        /// <summary>
        /// Name associated with the account
        /// </summary>
        public string Name { get; set; }

        public Guid? TenantId { get; set; }
    }
}

using MedEasy.Objects;
using System;
using System.Collections.Generic;

namespace Identity.Objects
{
    public class Claim : AuditableEntity<int, Claim>
    {
        /// <summary>
        /// Type of claim
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Value of the claim
        /// </summary>
        public string Value { get; set; }

        public IEnumerable<AccountClaim> Users { get; set; }

        public IEnumerable<RoleClaim> Roles { get; set; }
    }
}
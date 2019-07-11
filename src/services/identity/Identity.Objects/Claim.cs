using MedEasy.Objects;
using System;
using System.Collections.Generic;

namespace Identity.Objects
{
    public class Claim : AuditableEntity<Guid, Claim>
    {
        /// <summary>
        /// Type of claim
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Value of the claim
        /// </summary>
        public string Value { get; private set; }

        public IEnumerable<AccountClaim> Users { get; set; }

        public IEnumerable<RoleClaim> Roles { get; set; }

        public Claim(Guid id, string type, string value) : base(id)
        {
            Type = type;
            Value = value;
        }


        public void ChangeValueTo(string newValue) => Value = newValue;
    }
}
using MedEasy.Objects;
using System;

namespace Identity.Objects
{
    /// <summary>
    /// Associate a <see cref="Claim"/> to a <see cref="Account"/> for a period of time.
    /// </summary>
    /// <remarks>
    /// This association takes precedence over a <see cref="RoleClaim"/> association for a given <see cref="Claim"/>.
    /// </remarks>
    public class AccountClaim : AuditableEntity<int,AccountClaim>
    {
        public int ClaimId { get; set; }

        public int AccountId { get; set; }

        /// <summary>
        /// Overrides the <see cref="Claim"/>'s <see cref="Claim.Value"/> for the current <see cref="Account"/>
        /// </summary>
        public string Value { get; set; }

        public Account Account { get; set; }

        public Claim Claim { get; set; }

        /// <summary>
        /// When the claim is active for the user
        /// </summary>
        public DateTimeOffset Start { get; set; }

        /// <summary>
        /// When will the claim ends
        /// </summary>
        public DateTimeOffset? End { get; set; }
    }
}

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
    public class AccountClaim : AuditableEntity<Guid,AccountClaim>
    {
        public Guid ClaimId { get; private set; }

        public Guid AccountId { get; private set;  }

        /// <summary>
        /// Overrides the <see cref="Claim"/>'s <see cref="Claim.Value"/> for the current <see cref="Account"/>
        /// </summary>
        public string Value { get; private set; }

        public Account Account { get; private set; }

        public Claim Claim { get; private set; }

        /// <summary>
        /// When the claim is active for the user
        /// </summary>
        public DateTimeOffset Start { get; private set; }

        /// <summary>
        /// When will the claim ends
        /// </summary>
        public DateTimeOffset? End { get; }

        public AccountClaim(Guid id, Guid accountId, Guid claimId, string value, DateTimeOffset start, DateTimeOffset? end)
            : base(id)
        {
            Value = value;
            Start = start;
            End = end;
            ClaimId = claimId;
            AccountId = accountId;
        }

        public void ChangeValueTo(string newValue) => Value = newValue;
    }
}

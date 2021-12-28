namespace Identity.Objects
{
    using Identity.Ids;

    using MedEasy.Objects;

    using NodaTime;

    /// <summary>
    /// Associate a <see cref="Claim"/> to a <see cref="Account"/> for a period of time.
    /// </summary>
    /// <remarks>
    /// This association takes precedence over a <see cref="RoleClaim"/> association for a given <see cref="Claim"/>.
    /// </remarks>
    public class AccountClaim : Entity<AccountClaimId, AccountClaim>
    {
        /// <summary>
        /// Gets the id of the <see cref="Account"/>
        /// </summary>
        public AccountId AccountId { get; }

        /// <summary>
        /// Gets the claim referenced in the current relation
        /// </summary>
        public Claim Claim { get; }

        /// <summary>
        /// When the claim is active for the user
        /// </summary>
        public Instant Start { get; }

        /// <summary>
        /// When will the claim ends
        /// </summary>
        public Instant? End { get; }

        private AccountClaim(AccountId accountId, AccountClaimId id) : base(id)
        {
            AccountId = accountId;
        }

        /// <summary>
        /// Builds a new <see cref="AccountClaim"/> with the specified values
        /// </summary>
        /// <param name="accountId">Id of the <see cref="Account"/> which the current claim is set for</param>
        /// <param name="id">Id of the current record</param>
        /// <param name="type">type of the current account claim</param>
        /// <param name="value">Value of the current claim</param>
        /// <param name="start">Indicates when the claim start to be valid</param>
        /// <param name="end">Indicates when the claim's validity ends</param>
        public AccountClaim(AccountId accountId, AccountClaimId id, string type, string value, Instant start, Instant? end) : this(accountId, id)
        {
            AccountId = accountId;
            Claim = new Claim(type, value);
            Start = start;
            End = end;
        }

        /// <summary>
        /// Changes the value of the claim
        /// </summary>
        /// <param name="newValue"></param>
        public void ChangeValueTo(string newValue) => Claim.ChangeValueTo(newValue);
    }
}

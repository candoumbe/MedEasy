namespace Identity.CQRS.Events.Accounts
{
    using Identity.Ids;

    using MediatR;

    /// <summary>
    /// Event indicating an <see cref="AccountInfo"/> was deleted
    /// </summary>
    public class AccountDeleted : INotification
    {
        /// <summary>
        /// Id of the deleted account
        /// </summary>
        public AccountId AccountId { get; }

        /// <summary>
        /// Builds a new <see cref="AccountDeleted"/> instance
        /// </summary>
        /// <param name="accountId">id of the deleted account</param>
        public AccountDeleted(AccountId accountId) => AccountId = accountId;
    }
}

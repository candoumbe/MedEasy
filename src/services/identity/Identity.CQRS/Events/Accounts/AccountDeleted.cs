using MediatR;
using System;

namespace Identity.CQRS.Events.Accounts
{
    /// <summary>
    /// Event indicating an <see cref="AccountInfo"/> was deleted
    /// </summary>
    public class AccountDeleted : INotification
    {
        /// <summary>
        /// Id of the deleted account
        /// </summary>
        public Guid AccountId { get; }

        /// <summary>
        /// Builds a new <see cref="AccountDeleted"/> instance
        /// </summary>
        /// <param name="accountId">id of the deleted account</param>
        public AccountDeleted(Guid accountId) => AccountId = accountId;
    }
}

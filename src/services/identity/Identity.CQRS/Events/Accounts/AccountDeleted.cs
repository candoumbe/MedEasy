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
        public Guid Id { get; }

        /// <summary>
        /// Builds a new <see cref="AccountDeleted"/> instance
        /// </summary>
        /// <param name="id">id of the deleted account</param>
        public AccountDeleted(Guid id)
        {
            Id = id;
        }
    }
}

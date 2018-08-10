using Identity.DTO;
using MedEasy.CQRS.Core.Events;
using System;

namespace Identity.CQRS.Events.Accounts
{
    /// <summary>
    /// Event indicating an <see cref="AccountInfo"/> was created
    /// </summary>
    public class AccountCreated : NotificationBase<Guid, AccountInfo>
    {
        
        /// <summary>
        /// Builds a new <see cref="AccountCreated"/> instance
        /// </summary>
        /// <param name="accountInfo">the created account</param>
        public AccountCreated(AccountInfo accountInfo) : base(Guid.NewGuid(), accountInfo)
        { }
    }
}

namespace Identity.DTO
{
    using MedEasy.ValueObjects;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ConnectedAccountInfo
    {
        public Guid Id { get; set; }

        public virtual UserName Username { get; set; }

        public virtual Email Email { get; set; }

        /// <summary>
        /// Name of the account.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Claims associated with the account
        /// </summary>
        public IEnumerable<ClaimInfo> Claims { get; set; }

        public ConnectedAccountInfo() => Claims = Enumerable.Empty<ClaimInfo>();
    }

    public class DisconnectedAccountInfo : ConnectedAccountInfo
    {
        public override UserName Username { get => UserName.Empty; set => base.Username = value; }

        public override Email Email { get => Email.Empty; set => base.Email = value; }

        public override string Name { get => string.Empty; set => base.Name = value; }
    }
}

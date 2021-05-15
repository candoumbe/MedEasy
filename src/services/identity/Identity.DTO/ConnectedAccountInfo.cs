namespace Identity.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ConnectedAccountInfo
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// Name of the account.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Claims associated with the account
        /// </summary>
        public IEnumerable<ClaimInfo> Claims { get; set; }

        public ConnectedAccountInfo() => Claims = Enumerable.Empty<ClaimInfo>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.DTO
{
    public class AccountInfo
    {

        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public IEnumerable<ClaimInfo> Claims { get; set; }

        public AccountInfo() => Claims = Enumerable.Empty<ClaimInfo>();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Models.v1
{
    public class AccountModel
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public bool Locked { get; set; }

        /// <summary>
        /// Name associated with the account
        /// </summary>
        public string Name { get; set; }

    }
}

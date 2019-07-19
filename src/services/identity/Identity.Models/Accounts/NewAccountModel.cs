using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Identity.Models.Accounts
{
    public class NewAccountModel
    {
        /// <summary>
        /// Name associated with the account
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Desired username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Confirmation of the password
        /// </summary>
        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Email associated with the <see cref="AccountInfo"/>
        /// </summary>
        [Required]
        public string Email { get; set; }


        public Guid? TenantId { get; set; }
    }
}

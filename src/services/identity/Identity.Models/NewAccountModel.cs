using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class NewAccountModel
    {
        /// <summary>
        /// Name of the account to create
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Email of the account to create
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// Password of the account to create
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Confirmation of the password to create
        /// </summary>
        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword{ get; set; }
    }
}

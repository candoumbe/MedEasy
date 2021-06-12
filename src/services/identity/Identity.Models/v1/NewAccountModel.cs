
namespace Identity.Models.v1
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Model for creating an <see cref="Objects.Account"/>
    /// </summary>
    public class NewAccountModel
    {

        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

    }
}

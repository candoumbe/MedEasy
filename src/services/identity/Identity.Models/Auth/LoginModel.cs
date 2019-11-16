using System.ComponentModel.DataAnnotations;

namespace Identity.Models.Auth
{
    /// <summary>
    /// Model to submit when required access to API
    /// </summary>
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

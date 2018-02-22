using MedEasy.Objects;

namespace MedEasy.Identity.Objects
{
    public class User : AuditableEntity<int, User>
    {
        /// <summary>
        /// Used to access the application
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password associated with the account
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Email associated with the account
        /// </summary>
        public string Email { get; set; }
        
        public bool? EmailConfirmed { get; set; }
    }
}

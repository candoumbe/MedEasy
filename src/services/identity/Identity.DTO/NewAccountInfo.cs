using Identity.Ids;

using MedEasy.Ids;

namespace Identity.DTO
{
    /// <summary>
    /// Wraps 
    /// </summary>
    public class NewAccountInfo
    {
        public AccountId Id { get; set; }

        /// <summary>
        /// Name associated with the account
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Desired username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Confirmation of the password
        /// </summary>
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Email associated with the <see cref="AccountInfo"/>
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Owner of the account
        /// </summary>
        public TenantId TenantId { get; set; }
    }
}

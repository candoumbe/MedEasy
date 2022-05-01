namespace Identity.API.Features.v1.Auth
{
    using MedEasy.ValueObjects;

    /// <summary>
    /// Model used to get a new token
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// User's name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password associated with the account
        /// </summary>
        public Password Password { get; set; }
    }
}
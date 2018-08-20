namespace Identity.API.Features.Auth
{

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
        public string Password { get; set; }
    }
}
namespace Identity.DTO
{
    using Identity.ValueObjects;

    public class LoginInfo
    {
        public UserName Username { get; set; }

        public string Password { get; set; }
    }
}

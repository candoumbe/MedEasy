namespace Identity.DTO
{
    using Identity.ValueObjects;

    public class LoginInfo
    {
        public UserName UserName { get; set; }

        public string Password { get; set; }

    }
}

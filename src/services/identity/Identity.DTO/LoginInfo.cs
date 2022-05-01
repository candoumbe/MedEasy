namespace Identity.DTO
{
    using MedEasy.ValueObjects;

    public class LoginInfo
    {
        public UserName UserName { get; set; }

        public Password Password { get; set; }

    }
}

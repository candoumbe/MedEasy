namespace MedEasy.Objects
{
    public class User : AuditableEntity<int, User>

    {
        public string UserName { get; set; }

        public string PasswordHash { get; set; }

        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

    }
}

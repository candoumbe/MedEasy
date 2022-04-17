namespace MedEasy.Wasm.Apis.Identity
{
    public record AccountModel : BaseAuditableModel<Guid>
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string PreferredTimezone { get; set; }
    }

    /// <summary>
    /// Model used to update an account
    /// </summary>
    public record UpdateAccountModel
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string PreferredTimezone { get; set; }

    }
}

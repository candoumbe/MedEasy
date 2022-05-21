namespace MedEasy.Wasm.Apis.Identity
{
    using System.ComponentModel.DataAnnotations;

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

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        
        public string PreferredTimezone { get; set; }

    }
}

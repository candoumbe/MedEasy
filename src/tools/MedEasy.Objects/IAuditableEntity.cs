namespace MedEasy.Objects
{
    using NodaTime;

    public interface IAuditableEntity
    {
        Instant? CreatedDate { get; set; }

        string CreatedBy { get; set; }

        Instant? UpdatedDate { get; set; }

        string UpdatedBy { get; set; }
    }
}

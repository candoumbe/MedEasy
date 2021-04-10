using NodaTime;

namespace MedEasy.Objects
{
    public class AuditableBaseEntity<T> : BaseEntity<T>, IAuditableEntity
    {
        public Instant? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public Instant? UpdatedDate { get; set; }

        public string UpdatedBy { get; set; }
    }
}

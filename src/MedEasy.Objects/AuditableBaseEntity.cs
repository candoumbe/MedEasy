using System;

namespace MedEasy.Objects
{
    public class AuditableBaseEntity<T> : BaseEntity<T>, IAuditableEntity where T : class
    {
        public DateTimeOffset? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset? UpdatedDate { get; set; }

        public string UpdatedBy { get; set; }
    }
}

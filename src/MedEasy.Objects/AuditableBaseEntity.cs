using System;

namespace MedEasy.Objects
{
    public class AuditableBaseEntity<T> : BaseEntity<T>, IAuditableEntity where T : class
    {
        public DateTime? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedBy { get; set; }
    }
}

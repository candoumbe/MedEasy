using System;

namespace MedEasy.Objects
{
    public abstract class AuditableEntity<TKey, TEntry> : Entity<TKey, TEntry>, IAuditableEntity where TEntry : class
    {
        public DateTimeOffset? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset? UpdatedDate { get; set; }

        public string UpdatedBy { get; set; }
    }
}
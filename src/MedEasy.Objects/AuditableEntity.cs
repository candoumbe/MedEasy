using System;

namespace MedEasy.Objects
{
    public abstract class AuditableEntity<TKey, TEntry> : Entity<TKey, TEntry>, IAuditableEntity where TEntry : class
    {
        public DateTime? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedBy { get; set; }
    }
}
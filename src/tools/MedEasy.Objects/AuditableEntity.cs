namespace MedEasy.Objects
{
    using NodaTime;

    public abstract class AuditableEntity<TKey, TEntry> : Entity<TKey, TEntry>, IAuditableEntity where TEntry : class
    {
        public Instant? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public Instant? UpdatedDate { get; set; }

        public string UpdatedBy { get; set; }

        protected AuditableEntity(TKey id) : base(id) { }
    }
}
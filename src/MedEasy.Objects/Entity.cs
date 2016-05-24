namespace MedEasy.Objects
{
    public abstract class Entity<TKey, TEntry> : BaseEntity<TEntry>, IEntity<TKey> where TEntry : class
    {
        public virtual TKey Id { get; set; }

    }
}
using System;

namespace MedEasy.Objects
{
    public abstract class Entity<TKey, TEntry> : BaseEntity<TEntry>, IEntity<TKey> where TEntry : class
    {
        public virtual TKey Id { get; set; }

        public Guid UUID { get; set; }

        /// <summary>
        /// Builds a new <see cref="Entity{TKey, TEntry}"/>
        /// </summary>
        public Entity()
        {
            UUID = Guid.NewGuid();
        }
    }
}
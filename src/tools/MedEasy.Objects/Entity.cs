using System;

namespace MedEasy.Objects
{
    public abstract class Entity<TKey, TEntry> : BaseEntity<TEntry>, IEntity<TKey> where TEntry : class
    {
#pragma warning disable IDE0044 // Ajouter un modificateur readonly
        private TKey _id;
#pragma warning restore IDE0044 // Ajouter un modificateur readonly

        public TKey Id => _id;

#pragma warning disable IDE0044 // Ajouter un modificateur readonly
        private Guid _uuid;
#pragma warning restore IDE0044 // Ajouter un modificateur readonly

        public Guid UUID => _uuid;

        /// <summary>
        /// Builds a new <see cref="Entity{TKey, TEntry}"/>
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="uuid"/> is <c>Guid.Empty</c></exception>
        protected Entity(Guid uuid)
        {
            if (uuid == default)
            {
                throw new ArgumentException(nameof(uuid));
            }
            _uuid = uuid;
        }
    }
}
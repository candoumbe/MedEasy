namespace MedEasy.Objects
{
    using System;

    public abstract class Entity<TKey, TEntry> : BaseEntity<TKey> where TEntry : class
    {
        private readonly TKey _id;

        public override TKey Id => _id;

        /// <summary>
        /// Builds a new <see cref="Entity{TKey, TEntry}"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is <c>Guid.Empty</c></exception>
        protected Entity(TKey id)
        {
            if (Equals(id, default))
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
            _id = id;
        }
    }
}
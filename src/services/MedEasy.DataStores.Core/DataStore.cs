using MedEasy.DAL.Interfaces;
using MedEasy.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.DataStores.Core.Relational
{
    /// <summary>
    /// Base class for creating a datastore specialized class.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class DataStore<TContext> : DbContext, IDbContext where TContext : DbContext
    {
        /// <summary>
        /// Usual size for the "normal" text
        /// </summary>
        public static readonly int NormalTextLength = 255;

        /// <summary>
        /// Usual size for "short" text
        /// </summary>
        public static readonly int ShortTextLength = 50;

        public DataStore(DbContextOptions<TContext> options) : base(options)
        {

        }

        /// <summary>
        /// <see cref="DbContext.OnModelCreating(ModelBuilder)"/>
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.Relational().TableName = entity.DisplayName();

                if (typeof(IAuditableEntity).IsAssignableFrom(entity.ClrType))
                {
                    IAuditableEntity auditableEntity = entity as IAuditableEntity;

                    modelBuilder.Entity(entity.Name).Property(typeof(string), nameof(IAuditableEntity.CreatedBy))
                        .HasMaxLength(NormalTextLength);

                    modelBuilder.Entity(entity.Name).Property(typeof(string), nameof(IAuditableEntity.UpdatedBy))
                        .HasMaxLength(NormalTextLength);

                    modelBuilder.Entity(entity.Name).Property(typeof(DateTimeOffset), nameof(IAuditableEntity.UpdatedDate))
                        .IsConcurrencyToken();
                }

                if (entity.ClrType.IsAssignableToGenericType(typeof(IEntity<>)))
                {
                    modelBuilder.Entity(entity.Name)
                        .HasKey("_id");
                    modelBuilder.Entity(entity.Name)
                        .Property(typeof(Guid), "_id");
                }
            }
        }

        private IEnumerable<EntityEntry> GetModifiedEntities()
     => ChangeTracker.Entries()
         .AsParallel()
         .Where(x => typeof(IAuditableEntity).IsAssignableFrom(x.Entity.GetType())
             && (x.State == EntityState.Added || x.State == EntityState.Modified))
#if DEBUG
            .ToArray()
#endif
            ;

        private Action<EntityEntry> UpdateModifiedEntry
            => x =>
            {
                IAuditableEntity auditableEntity = (IAuditableEntity)(x.Entity);
                DateTimeOffset now = DateTimeOffset.UtcNow;
                if (x.State == EntityState.Added)
                {
                    auditableEntity.CreatedDate = now;
                    auditableEntity.UpdatedDate = now;
                }
                else if (x.State == EntityState.Modified)
                {
                    auditableEntity.UpdatedDate = now;
                }
            };

        /// <summary>
        /// <see cref="DbContext.SaveChanges()"/>
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges() => SaveChanges(true);

        /// <summary>
        /// <see cref="DbContext.SaveChanges(bool)"/>
        /// </summary>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();
            entities
                .AsParallel()
                .ForEach(UpdateModifiedEntry);

            return SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// <see cref="DbContext.SaveChangesAsync(bool, CancellationToken)"/>
        /// </summary>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();

            entities
                .ForEach(UpdateModifiedEntry);

            return await base.SaveChangesAsync(true, ct)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            await SaveChangesAsync(true, ct)
                .ConfigureAwait(false);
    }
}

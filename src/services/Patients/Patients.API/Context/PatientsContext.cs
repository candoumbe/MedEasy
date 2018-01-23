using MedEasy.DAL.Interfaces;
using MedEasy.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Patients.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Patients.API.Context
{
    public class PatientsContext : DbContext, IDbContext
    {
        /// <summary>
        /// Usual size for the "normal" text
        /// </summary>
        private const int _normalTextLength = 255;

        /// <summary>
        /// Usual size for "short" text
        /// </summary>
        private const int _shortTextLength = 50;

        /// <summary>
        /// Collection of <see cref="Patient"/>s
        /// </summary>
        public DbSet<Patient> Patients { get; set; }



        /// <summary>
        /// Builds a new <see cref="PatientsContext"/> instance.
        /// </summary>
        /// <param name="options">options of the MeasuresContext</param>
        public PatientsContext(DbContextOptions<PatientsContext> options) : base(options)
        {
        }

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
                        .HasMaxLength(_normalTextLength);

                    modelBuilder.Entity(entity.Name).Property(typeof(string), nameof(IAuditableEntity.UpdatedBy))
                        .HasMaxLength(_normalTextLength);

                    modelBuilder.Entity(entity.Name).Property(typeof(DateTimeOffset), nameof(IAuditableEntity.UpdatedDate))
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                }

                if (entity.ClrType.IsAssignableToGenericType(typeof(IEntity<>)))
                {
                    modelBuilder.Entity(entity.Name).Property(typeof(Guid), nameof(IEntity<int>.UUID))
                        .ValueGeneratedOnAdd();
                    modelBuilder.Entity(entity.Name)
                        .HasIndex(nameof(IEntity<int>.UUID))
                        .IsUnique();
                }

            }

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(x => x.Firstname)
                    .HasMaxLength(_normalTextLength);

                entity.Property(x => x.Lastname)
                    .HasMaxLength(_normalTextLength);

            });  


        }

        private IEnumerable<EntityEntry> GetModifiedEntities()
            => ChangeTracker.Entries()
                .AsParallel()
                .Where(x => x.Entity.GetType().IsAssignableFrom(typeof(IAuditableEntity)) && (x.State == EntityState.Added || x.State == EntityState.Modified));


        private Action<EntityEntry> UpdateModifiedEntry
            => x =>
            {
                IAuditableEntity auditableEntity = (IAuditableEntity)x;
                DateTimeOffset now = DateTimeOffset.UtcNow;
                auditableEntity.UpdatedDate = now;
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
        public override int SaveChanges()
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();
            entities.ForEach(UpdateModifiedEntry);
            return base.SaveChanges();
        }

        /// <summary>
        /// <see cref="DbContext.SaveChanges(bool)"/>
        /// </summary>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();
            entities
                .AsParallel()
                .ForEach(UpdateModifiedEntry);

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// <see cref="DbContext.SaveChangesAsync(bool, CancellationToken)"/>
        /// </summary>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();

            entities
                .AsParallel()
                .ForEach(UpdateModifiedEntry);


            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();

            entities
                .AsParallel()
                .ForEach(UpdateModifiedEntry);


            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}

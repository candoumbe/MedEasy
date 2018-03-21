using Measures.Objects;
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

namespace Measures.Context
{
    /// <summary>
    /// Interacts with the underlying repostories.
    /// </summary>
    public class MeasuresContext : DbContext, IDbContext
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
        /// Collection of <see cref="BloodPressure"/>s
        /// </summary>
        public DbSet<BloodPressure> BloodPressures { get; set; }

        /// <summary>
        /// Collection of <see cref="PhysiologicalMeasurement"/>
        /// </summary>
        public DbSet<PhysiologicalMeasurement> Measures { get; set; }

        /// <summary>
        /// Collection of <see cref="Temperature"/>s
        /// </summary>
        public DbSet<Temperature> Temperatures { get; set; }


        /// <summary>
        /// Builds a new <see cref="MeasuresContext"/> instance.
        /// </summary>
        /// <param name="options">options of the MeasuresContext</param>
        public MeasuresContext(DbContextOptions<MeasuresContext> options) : base(options)
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
                        .HasMaxLength(_normalTextLength);

                    modelBuilder.Entity(entity.Name).Property(typeof(string), nameof(IAuditableEntity.UpdatedBy))
                        .HasMaxLength(_normalTextLength);

                    modelBuilder.Entity(entity.Name).Property(typeof(DateTimeOffset), nameof(IAuditableEntity.UpdatedDate))
                        .IsConcurrencyToken();

                }

                if (entity.ClrType.IsAssignableToGenericType(typeof(IEntity<>)))
                {
                    modelBuilder.Entity(entity.Name).Property(typeof(Guid), nameof(IEntity<int>.UUID))
                        .ValueGeneratedOnAdd();
                    modelBuilder.Entity(entity.Name)
                        .HasIndex(nameof(IEntity<int>.UUID))
                        .IsUnique();

                    modelBuilder.Entity(entity.Name).Property(nameof(IEntity<int>.Id))
                       .ValueGeneratedOnAdd();
                }

            }


            modelBuilder.Entity<PhysiologicalMeasurement>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Patient);
            });

            modelBuilder.Entity<PhysiologicalMeasurement>()
                .ToTable(nameof(Measures));

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(x => x.Firstname)
                    .HasMaxLength(_normalTextLength);

                entity.Property(x => x.Lastname)
                    .HasMaxLength(_normalTextLength);

            });

            modelBuilder.Entity<BloodPressure>();

            modelBuilder.Entity<Temperature>();
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
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();

            entities
                .ForEach(UpdateModifiedEntry);

            return await base.SaveChangesAsync(true, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await SaveChangesAsync(true, cancellationToken)
                .ConfigureAwait(false);
    }
}

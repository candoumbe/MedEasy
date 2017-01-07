﻿using System.Linq;
using MedEasy.DAL.Interfaces;
using MedEasy.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.API.Stores
{
    /// <summary>
    /// A <see cref="MedEasyContext"/> instance represents a session with all the repositories
    /// </summary>
    public class MedEasyContext : DbContext, IDbContext
    {
        /// <summary>
        /// Usual size for the "normal" text
        /// </summary>
        private const int NormalTextLength = 256;

        /// <summary>
        /// Usual size for "short" text
        /// </summary>
        private const int ShortTextLength = 50;

        /// <summary>
        /// Gives access to <see cref="Doctor"/> entities
        /// </summary>
        public DbSet<Doctor> Doctors { get; set; }
        /// <summary>
        /// Gives access to <see cref="Specialty"/> entities
        /// </summary>
        public DbSet<Specialty> Specialties { get; set; }

        /// <summary>
        /// Gives access to <see cref="Patient"/> entities
        /// </summary>
        public DbSet<Patient> Patients { get; set; }

        /// <summary>
        /// Gives access to <see cref="BloodPressure"/> entities
        /// </summary>
        public DbSet<BloodPressure> BloodPressures { get; set; }

        /// <summary>
        /// Gives access to <see cref="Temperature"/> entities
        /// </summary>
        public DbSet<Temperature> Temperatures { get; set; }

        /// <summary>
        /// Gives access to <see cref="BodyWeight"/>
        /// </summary>
        public DbSet<BodyWeight> BodyWeights { get; set; }

        /// <summary>
        /// Gives access to <see cref="Prescription"/> entities.
        /// </summary>
        public DbSet<Prescription> Prescriptions { get; set; }

        

        /// <summary>
        /// Builds a new instance of <see cref="MedEasyContext"/> with default options
        /// </summary>
        public MedEasyContext()
        {  
        }


        /// <summary>
        /// Builds à new instance of <see cref="MedEasyContext"/> with the specified <see cref="DbContextOptions{TContext}"/>
        /// </summary>
        /// <param name="options">options to customize the context behaviour</param>
        public MedEasyContext(DbContextOptions<MedEasyContext> options) : base(options)
        {}

        ///<inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MedEasy;Trusted_Connection=True;MultipleActiveResultSets=true");
            }

            base.OnConfiguring(optionsBuilder);
            
        }

        ///<inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.Relational().TableName = entity.DisplayName();
                
                if (entity.ClrType is IAuditableEntity)
                {

                    IAuditableEntity auditableEntity = entity as IAuditableEntity;

                    modelBuilder.Entity(entity.Name).Property(typeof(string), nameof(IAuditableEntity.CreatedBy))
                        .HasMaxLength(NormalTextLength);

                    modelBuilder.Entity(entity.Name).Property(typeof(string), nameof(IAuditableEntity.UpdatedBy))
                        .HasMaxLength(NormalTextLength);
                }

            }

            
            #region Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(item => item.Id)
                    .UseSqlServerIdentityColumn()
                    .ValueGeneratedOnAdd();

                entity.Property(item => item.Firstname)
                    .HasMaxLength(NormalTextLength)
                    .HasDefaultValue(string.Empty)
                    .IsRequired();

                entity.Property(item => item.Lastname)
                    .HasMaxLength(NormalTextLength)
                    .IsRequired();

                entity.Property(x => x.Notes)
                    .HasDefaultValue(string.Empty)
                    .IsRequired();

                
                entity.HasIndex(item => item.Lastname);
                entity.HasIndex(item => item.BirthDate);




                entity.Property(item => item.UpdatedDate)
                    .IsConcurrencyToken();




            });

            #endregion

            #region Doctor

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.Property(item => item.Id)
                    .UseSqlServerIdentityColumn()
                    .ValueGeneratedOnAdd();

                entity.Property(item => item.Firstname)
                    .HasMaxLength(NormalTextLength)
                    .HasDefaultValue(string.Empty)
                    .IsRequired();

                entity.Property(item => item.Lastname)
                    .HasMaxLength(NormalTextLength)
                    .IsRequired();

                entity.HasIndex(item => item.Lastname);

                entity.Property(item => item.UpdatedDate)
                    .IsConcurrencyToken();
            });

            #endregion

            #region Specialty
            modelBuilder.Entity<Specialty>(entity =>
            {
                entity.HasKey(item => item.Id);

                entity.Property(item => item.Id)
                    .UseSqlServerIdentityColumn()
                    .ValueGeneratedOnAdd();

                entity.Property(item => item.Name)
                    .HasMaxLength(NormalTextLength)
                    .IsRequired();

                entity.Property(item => item.UpdatedDate)
                    .IsConcurrencyToken();
            });

            #endregion


            

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
                }
            };

        

        public override int SaveChanges()
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();
            entities.ForEach(UpdateModifiedEntry);
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();
            entities
                .AsParallel()
                .ForEach(UpdateModifiedEntry);

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();

            entities
                .AsParallel()
                .ForEach(UpdateModifiedEntry);


            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<EntityEntry> entities = GetModifiedEntities();

            entities
                .AsParallel()
                .ForEach(UpdateModifiedEntry);


            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}

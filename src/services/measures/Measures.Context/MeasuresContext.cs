using Measures.Objects;
using MedEasy.DAL.Interfaces;
using MedEasy.DataStores.Core.Relational;
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
    public class MeasuresContext : DataStore<MeasuresContext>
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


            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name)
                    .HasMaxLength(_normalTextLength);
            });

            modelBuilder.Entity<BloodPressure>(entity => {
                entity.Property(x => x.SystolicPressure);
                entity.Property(x => x.DiastolicPressure);
            });
        }

    }
}

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
using MedEasy.DataStores.Core.Relational;

namespace Patients.Context
{
    public class PatientsContext : DataStore<PatientsContext>
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

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(x => x.Firstname)
                    .HasMaxLength(_normalTextLength);

                entity.Property(x => x.Lastname)
                    .HasMaxLength(_normalTextLength);
            });
        }
    }
}

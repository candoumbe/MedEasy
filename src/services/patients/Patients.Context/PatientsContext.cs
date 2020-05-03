using MedEasy.DataStores.Core.Relational;

using Microsoft.EntityFrameworkCore;

using Patients.Objects;

namespace Patients.Context
{
    public class PatientsContext : DataStore<PatientsContext>
    {
        /// <summary>
        /// Usual size for the "normal" text
        /// </summary>
        private const int _normalTextLength = 255;

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

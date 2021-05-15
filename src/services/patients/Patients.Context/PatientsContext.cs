namespace Patients.Context
{
    using MedEasy.DataStores.Core.Relational;

    using Microsoft.EntityFrameworkCore;

    using NodaTime;

    using Patients.Objects;

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
        /// <param name="clock"><see cref="IClock"/> instance used to access current date/time</param>
        public PatientsContext(DbContextOptions<PatientsContext> options, IClock clock) : base(options, clock)
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

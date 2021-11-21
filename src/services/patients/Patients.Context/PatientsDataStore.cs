namespace Patients.Context
{
    using MedEasy.DataStores.Core.Relational;

    using Microsoft.EntityFrameworkCore;

    using NodaTime;

    using Patients.Objects;

    /// <summary>
    /// <see cref="DataStore"/> implementation
    /// </summary>
    public class PatientsDataStore : DataStore<PatientsDataStore>
    {
        /// <summary>
        /// Usual size for the "normal" text
        /// </summary>
        private const int _normalTextLength = 255;

        /// <summary>
        /// Collection of <see cref="Patient"/> entities
        /// </summary>
        public DbSet<Patient> Patients { get; set; }

        /// <summary>
        /// Collection of <see cref="Doctor"/> entities
        /// </summary>
        public DbSet<Doctor> Doctors { get; set; }

        /// <summary>
        /// Builds a new <see cref="PatientsDataStore"/> instance.
        /// </summary>
        /// <param name="options">options of the MeasuresContext</param>
        /// <param name="clock"><see cref="IClock"/>service used to access current date/time</param>
        public PatientsDataStore(DbContextOptions<PatientsDataStore> options, IClock clock) : base(options, clock)
        {
        }

        ///<inheritdoc/>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Patient>(entity =>
            {
                entity.Property(x => x.Firstname)
                    .HasMaxLength(_normalTextLength);

                entity.Property(x => x.Lastname)
                    .HasMaxLength(_normalTextLength);
            });

            builder.Entity<Doctor>(entity =>
            {
                entity.Property(x => x.Firstname)
                      .HasMaxLength(_normalTextLength);

                entity.Property(x => x.Lastname)
                      .HasMaxLength(_normalTextLength);
            });
        }
    }
}

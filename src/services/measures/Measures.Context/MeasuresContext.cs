using Measures.Objects;

using MedEasy.DataStores.Core.Relational;

using Microsoft.EntityFrameworkCore;

using NodaTime;

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
        /// Builds a new <see cref="MeasuresContext"/> instance.
        /// </summary>
        /// <param name="options">options of the MeasuresContext</param>
        /// <param name="clock"><see cref="IClock"/> implementation used when current time is needed</param>
        public MeasuresContext(DbContextOptions<MeasuresContext> options, IClock clock) : base(options, clock)
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

                entity.Property(x => x.BirthDate);

                entity.HasMany(x => x.BloodPressures)
                      .WithOne(x => x.Patient)
                      .HasForeignKey(measure => measure.PatientId)
                      .HasPrincipalKey(patient => patient.Id);

                entity.HasMany(x => x.Temperatures)
                      .WithOne(x => x.Patient)
                      .HasForeignKey(measure => measure.PatientId)
                      .HasPrincipalKey(patient => patient.Id);
            });

            modelBuilder.Entity<BloodPressure>(entity => {
                entity.HasKey(x => new { x.PatientId, x.DateOfMeasure });
                entity.HasIndex(x => x.Id);
            });

            modelBuilder.Entity<Temperature>(entity => {
                entity.HasKey(x => new { x.PatientId, x.DateOfMeasure });
                entity.HasIndex(x => x.Id);
            });
        }
    }
}

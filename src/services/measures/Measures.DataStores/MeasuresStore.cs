namespace Measures.DataStores
{
    using Measures.Objects;

    using MedEasy.DataStores.Core.Relational;

    using Microsoft.EntityFrameworkCore;

    using NodaTime;

    /// <summary>
    /// Interacts with the underlying repostories.
    /// </summary>
    public class MeasuresStore : DataStore<MeasuresStore>
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
        /// Builds a new <see cref="MeasuresStore"/> instance.
        /// </summary>
        /// <param name="options">options of the MeasuresContext</param>
        /// <param name="clock"><see cref="IClock"/> implementation used when current time is needed</param>
        public MeasuresStore(DbContextOptions<MeasuresStore> options, IClock clock) : base(options, clock)
        {
        }

        /// <summary>
        /// <see cref="DbContext.OnModelCreating(ModelBuilder)"/>
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Subject>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name)
                    .HasMaxLength(_normalTextLength);

                entity.Property(x => x.BirthDate);

                entity.HasMany(x => x.BloodPressures)
                      .WithOne(x => x.Subject)
                      .HasForeignKey(measure => measure.SubjectId)
                      .HasPrincipalKey(patient => patient.Id);

                entity.HasMany(x => x.Temperatures)
                      .WithOne(x => x.Subject)
                      .HasForeignKey(measure => measure.SubjectId)
                      .HasPrincipalKey(patient => patient.Id);
            });

            builder.Entity<BloodPressure>(entity =>
            {
                entity.HasKey(x => new { x.SubjectId, x.DateOfMeasure });
                entity.HasIndex(x => x.Id);
            });

            builder.Entity<Temperature>(entity =>
            {
                entity.HasKey(x => new { x.SubjectId, x.DateOfMeasure });
                entity.HasIndex(x => x.Id);
            });
        }
    }
}

namespace Agenda.DataStores
{
    using Agenda.Objects;

    using MedEasy.DataStores.Core.Relational;

    using Microsoft.EntityFrameworkCore;

    using NodaTime;

    /// <summary>
    /// Interacts with the underlying repostories.
    /// </summary>
    public class AgendaContext : DataStore<AgendaContext>
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
        /// Collection of <see cref="Attendee"/>s
        /// </summary>
        public DbSet<Attendee> Participants { get; set; }

        /// <summary>
        /// Collection of <see cref="Appointment"/>
        /// </summary>
        public DbSet<Appointment> Appointments { get; set; }

        /// <summary>
        /// Builds a new <see cref="AgendaContext"/> instance.
        /// </summary>
        /// <param name="options">options of the MeasuresContext</param>
        public AgendaContext(DbContextOptions<AgendaContext> options, IClock clock) : base(options, clock)
        {
        }

        /// <summary>
        /// <see cref="DbContext.OnModelCreating(ModelBuilder)"/>
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.Property(x => x.Id);

                entity.Property(x => x.Location)
                    .HasMaxLength(_normalTextLength);

                entity.Property(x => x.Subject)
                    .HasMaxLength(_normalTextLength)
                    .IsRequired();

                entity.Property(x => x.StartDate)
                      .IsRequired();

                entity.Property(x => x.EndDate)
                      .IsRequired();

                entity.HasMany(x => x.Attendees)
                      .WithMany(x => x.Appointments)
                      .UsingEntity(j => j.ToTable("AppointmentAttendee"));
            })
                .Entity<Attendee>(entity =>
            {
                entity.Property(x => x.Name)
                    .HasMaxLength(_normalTextLength)
                    .IsRequired();

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(_normalTextLength)
                    .IsRequired()
                    .HasDefaultValue(string.Empty);
            });
        }
    }
}

using Agenda.Objects;
using MedEasy.DAL.Interfaces;
using MedEasy.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MedEasy.DataStores.Core.Relational;

namespace Agenda.DataStores
{
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
        /// Collection of <see cref="BloodPressure"/>s
        /// </summary>
        public DbSet<Participant> Participants { get; set; }

        /// <summary>
        /// Collection of <see cref="Appointment"/>
        /// </summary>
        public DbSet<Appointment> Appointments { get; set; }

        /// <summary>
        /// Builds a new <see cref="AgendaContext"/> instance.
        /// </summary>
        /// <param name="options">options of the MeasuresContext</param>
        public AgendaContext(DbContextOptions<AgendaContext> options) : base(options)
        {
        }

        /// <summary>
        /// <see cref="DbContext.OnModelCreating(ModelBuilder)"/>
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppointmentParticipant>()
                .HasKey(ap => new { ap.AppointmentId, ap.ParticipantId });

            modelBuilder.Entity<AppointmentParticipant>()
                .HasOne(ap => ap.Appointment)
                .WithMany(a => a.Participants)
                .HasForeignKey(ap => ap.AppointmentId);

            modelBuilder.Entity<AppointmentParticipant>()
                .HasOne(ap => ap.Participant)
                .WithMany(a => a.Appointments)
                .HasForeignKey(ap => ap.ParticipantId);

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.Property(x => x.Location)
                    .HasMaxLength(_normalTextLength);

                entity.Property(x => x.Subject)
                    .HasMaxLength(_normalTextLength)
                    .IsRequired();

                entity.Property(x => x.StartDate)
                    .IsRequired();

                entity.Property(x => x.EndDate)
                    .IsRequired();
            });

            modelBuilder.Entity<Participant>(entity =>
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

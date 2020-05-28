using Forms;

using Measures.Objects;

using MedEasy.DataStores.Core.Relational;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System.Collections.Generic;
using System.Text.Json;

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

        private const string PostgresProviderName = "Npqsql.EntityFrameworkCore.PostgreSQL";

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

                entity.HasMany(x => x.BloodPressures)
                      .WithOne(x => x.Patient)
                      .HasForeignKey(measure => measure.PatientId)
                      .HasPrincipalKey(patient => patient.Id);

                entity.HasMany(x => x.Temperatures)
                      .WithOne(x => x.Patient)
                      .HasForeignKey(measure => measure.PatientId)
                      .HasPrincipalKey(patient => patient.Id);

                entity.HasMany(x => x.Measures)
                      .WithOne(x => x.Patient)
                      .HasForeignKey(measure => measure.PatientId)
                      .HasPrincipalKey(patient => patient.Id);



            });

            modelBuilder.Entity<GenericMeasure>(entity =>
            {
                //measure.WithOwner(x => x.Patient)
                entity.Property(x => x.FormId);
                PropertyBuilder<JsonDocument> dataPropertyBuilder = entity.Property(x => x.Data)
                                                                            .IsRequired();

                if (Database.ProviderName != PostgresProviderName)
                {
                    dataPropertyBuilder.HasConversion<string>(new ValueConverter<JsonDocument, string>(
                        document => document.RootElement.GetRawText(),
                        json => JsonDocument.Parse(json, default)
                    ));
                }

                entity.HasKey(x => new { x.PatientId, x.Id });
            });
            modelBuilder.Entity<BloodPressure>(entity =>
            {
                entity.HasKey(x => new { x.PatientId, x.DateOfMeasure });
                entity.HasIndex(x => x.Id);
            });

            modelBuilder.Entity<Temperature>(entity =>
            {
                entity.HasKey(x => new { x.PatientId, x.DateOfMeasure });
                entity.HasIndex(x => x.Id);
            });

            modelBuilder.Entity<MeasureForm>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name)
                      .HasMaxLength(_shortTextLength)
                      .IsRequired();
                PropertyBuilder fieldsPropertyBuilder = entity.Property(x => x.Fields)
                      .IsRequired();

                if (Database.ProviderName == PostgresProviderName)
                {
                    fieldsPropertyBuilder.HasColumnType("jsonb");
                }
                else
                {
                    fieldsPropertyBuilder.HasConversion(new ValueConverter<IEnumerable<FormField>, string>(
                        fields => JsonSerializer.Serialize(fields, null),
                        json => JsonSerializer.Deserialize<IEnumerable<FormField>>(json, null)
                    ));
                }
            });
        }
    }
}

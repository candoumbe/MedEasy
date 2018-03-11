﻿// <auto-generated />
using Measures.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;

namespace Measures.API.Migrations
{
    [DbContext(typeof(MeasuresContext))]
    [Migration("20180310234837_Added_TemperatureEntity")]
    partial class Added_TemperatureEntity
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Measures.Objects.Patient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("BirthDate");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("CreatedDate");

                    b.Property<string>("Firstname")
                        .HasMaxLength(255);

                    b.Property<string>("Lastname")
                        .HasMaxLength(255);

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("UpdatedDate")
                        .IsConcurrencyToken();

                    b.HasKey("Id");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("Patient");
                });

            modelBuilder.Entity("Measures.Objects.PhysiologicalMeasurement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("CreatedDate");

                    b.Property<DateTimeOffset>("DateOfMeasure");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<int>("PatientId");

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("UpdatedDate")
                        .IsConcurrencyToken();

                    b.HasKey("Id");

                    b.HasIndex("PatientId");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("Measures");

                    b.HasDiscriminator<string>("Discriminator").HasValue("PhysiologicalMeasurement");
                });

            modelBuilder.Entity("Measures.Objects.BloodPressure", b =>
                {
                    b.HasBaseType("Measures.Objects.PhysiologicalMeasurement");

                    b.Property<float>("DiastolicPressure");

                    b.Property<float>("SystolicPressure");

                    b.ToTable("BloodPressure");

                    b.HasDiscriminator().HasValue("BloodPressure");
                });

            modelBuilder.Entity("Measures.Objects.Temperature", b =>
                {
                    b.HasBaseType("Measures.Objects.PhysiologicalMeasurement");

                    b.Property<float>("Value");

                    b.ToTable("Temperature");

                    b.HasDiscriminator().HasValue("Temperature");
                });

            modelBuilder.Entity("Measures.Objects.PhysiologicalMeasurement", b =>
                {
                    b.HasOne("Measures.Objects.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}

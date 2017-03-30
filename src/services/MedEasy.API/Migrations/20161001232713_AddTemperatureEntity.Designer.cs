using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MedEasy.API.Stores;

namespace MedEasy.API.Migrations
{
    [DbContext(typeof(MedEasyContext))]
    [Migration("20161001232713_AddTemperatureEntity")]
    partial class AddTemperatureEntity
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MedEasy.Objects.BloodPressure", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedDate");

                    b.Property<DateTime>("DateOfMeasure");

                    b.Property<float>("DiastolicPressure");

                    b.Property<int>("PatientId");

                    b.Property<float>("SystolicPressure");

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime?>("UpdatedDate");

                    b.HasKey("Id");

                    b.HasIndex("PatientId");

                    b.ToTable("BloodPressure");
                });

            modelBuilder.Entity("MedEasy.Objects.Doctor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedDate");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<int?>("SpecialtyId");

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime?>("UpdatedDate")
                        .IsConcurrencyToken();

                    b.HasKey("Id");

                    b.HasIndex("Lastname");

                    b.HasIndex("SpecialtyId");

                    b.ToTable("Doctor");
                });

            modelBuilder.Entity("MedEasy.Objects.Patient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("BirthDate");

                    b.Property<string>("BirthPlace");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedDate");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<int?>("MainDoctorId");

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime?>("UpdatedDate")
                        .IsConcurrencyToken();

                    b.HasKey("Id");

                    b.HasIndex("BirthDate");

                    b.HasIndex("Lastname");

                    b.HasIndex("MainDoctorId");

                    b.ToTable("Patient");
                });

            modelBuilder.Entity("MedEasy.Objects.Specialty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 5);

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedDate");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime?>("UpdatedDate")
                        .IsConcurrencyToken();

                    b.HasKey("Id");

                    b.ToTable("Specialty");
                });

            modelBuilder.Entity("MedEasy.Objects.Temperature", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime?>("CreatedDate");

                    b.Property<DateTime>("DateOfMeasure");

                    b.Property<int>("PatientId");

                    b.Property<string>("UpdatedBy");

                    b.Property<DateTime?>("UpdatedDate");

                    b.Property<float>("Value");

                    b.HasKey("Id");

                    b.HasIndex("PatientId");

                    b.ToTable("Temperature");
                });

            modelBuilder.Entity("MedEasy.Objects.BloodPressure", b =>
                {
                    b.HasOne("MedEasy.Objects.Patient", "Patient")
                        .WithMany("BloodPressures")
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MedEasy.Objects.Doctor", b =>
                {
                    b.HasOne("MedEasy.Objects.Specialty", "Specialty")
                        .WithMany("Doctors")
                        .HasForeignKey("SpecialtyId");
                });

            modelBuilder.Entity("MedEasy.Objects.Patient", b =>
                {
                    b.HasOne("MedEasy.Objects.Doctor", "MainDoctor")
                        .WithMany("Patients")
                        .HasForeignKey("MainDoctorId");
                });

            modelBuilder.Entity("MedEasy.Objects.Temperature", b =>
                {
                    b.HasOne("MedEasy.Objects.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}

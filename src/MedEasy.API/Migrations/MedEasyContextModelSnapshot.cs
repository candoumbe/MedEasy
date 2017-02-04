using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MedEasy.API.Stores;

namespace MedEasy.API.Migrations
{
    [DbContext(typeof(MedEasyContext))]
    partial class MedEasyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MedEasy.Objects.Appointment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<int>("DoctorId");

                    b.Property<double>("Duration");

                    b.Property<int>("PatientId");

                    b.Property<DateTimeOffset>("StartDate");

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("DoctorId");

                    b.HasIndex("PatientId");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("Appointment");
                });

            modelBuilder.Entity("MedEasy.Objects.BloodPressure", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<DateTimeOffset>("DateOfMeasure");

                    b.Property<float>("DiastolicPressure");

                    b.Property<int>("PatientId");

                    b.Property<float>("SystolicPressure");

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("PatientId");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("BloodPressure");
                });

            modelBuilder.Entity("MedEasy.Objects.BodyWeight", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<DateTimeOffset>("DateOfMeasure");

                    b.Property<int>("PatientId");

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<decimal>("Value");

                    b.HasKey("Id");

                    b.HasIndex("PatientId");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("BodyWeight");
                });

            modelBuilder.Entity("MedEasy.Objects.Doctor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<int?>("SpecialtyId");

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("Lastname");

                    b.HasIndex("SpecialtyId");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("Doctor");
                });

            modelBuilder.Entity("MedEasy.Objects.Document", b =>
                {
                    b.Property<int>("DocumentMetadataId");

                    b.Property<byte[]>("Content")
                        .IsRequired();

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("DocumentMetadataId");

                    b.ToTable("Document");
                });

            modelBuilder.Entity("MedEasy.Objects.DocumentMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<int>("DocumentId");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<int>("PatientId");

                    b.Property<long>("Size");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("MimeType");

                    b.HasIndex("PatientId");

                    b.HasIndex("Title");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("DocumentMetadata");
                });

            modelBuilder.Entity("MedEasy.Objects.Patient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset?>("BirthDate");

                    b.Property<string>("BirthPlace");

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<int?>("MainDoctorId");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("");

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("BirthDate");

                    b.HasIndex("Lastname");

                    b.HasIndex("MainDoctorId");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("Patient");
                });

            modelBuilder.Entity("MedEasy.Objects.Prescription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<DateTimeOffset>("DeliveryDate");

                    b.Property<int>("PatientId");

                    b.Property<int>("PrescriptorId");

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("PatientId");

                    b.HasIndex("PrescriptorId");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("Prescription");
                });

            modelBuilder.Entity("MedEasy.Objects.PrescriptionItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Category");

                    b.Property<string>("Code");

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<string>("Designation");

                    b.Property<string>("Notes");

                    b.Property<int?>("PrescriptionId");

                    b.Property<decimal>("Quantity");

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("PrescriptionId");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("PrescriptionItem");
                });

            modelBuilder.Entity("MedEasy.Objects.Specialty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("Specialty");
                });

            modelBuilder.Entity("MedEasy.Objects.Temperature", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("CreatedDate");

                    b.Property<DateTimeOffset>("DateOfMeasure");

                    b.Property<int>("PatientId");

                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UpdatedBy")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<DateTimeOffset?>("UpdatedDate")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<float>("Value");

                    b.HasKey("Id");

                    b.HasIndex("PatientId");

                    b.HasIndex("UUID")
                        .IsUnique();

                    b.ToTable("Temperature");
                });

            modelBuilder.Entity("MedEasy.Objects.Appointment", b =>
                {
                    b.HasOne("MedEasy.Objects.Doctor", "Doctor")
                        .WithMany("Appointments")
                        .HasForeignKey("DoctorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MedEasy.Objects.Patient", "Patient")
                        .WithMany("Appointments")
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MedEasy.Objects.BloodPressure", b =>
                {
                    b.HasOne("MedEasy.Objects.Patient", "Patient")
                        .WithMany("BloodPressures")
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MedEasy.Objects.BodyWeight", b =>
                {
                    b.HasOne("MedEasy.Objects.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MedEasy.Objects.Doctor", b =>
                {
                    b.HasOne("MedEasy.Objects.Specialty", "Specialty")
                        .WithMany("Doctors")
                        .HasForeignKey("SpecialtyId");
                });

            modelBuilder.Entity("MedEasy.Objects.Document", b =>
                {
                    b.HasOne("MedEasy.Objects.DocumentMetadata", "DocumentMetadata")
                        .WithOne("Document")
                        .HasForeignKey("MedEasy.Objects.Document", "DocumentMetadataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MedEasy.Objects.DocumentMetadata", b =>
                {
                    b.HasOne("MedEasy.Objects.Patient", "Patient")
                        .WithMany("Documents")
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MedEasy.Objects.Patient", b =>
                {
                    b.HasOne("MedEasy.Objects.Doctor", "MainDoctor")
                        .WithMany("Patients")
                        .HasForeignKey("MainDoctorId");
                });

            modelBuilder.Entity("MedEasy.Objects.Prescription", b =>
                {
                    b.HasOne("MedEasy.Objects.Patient", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MedEasy.Objects.Doctor", "Prescriptor")
                        .WithMany()
                        .HasForeignKey("PrescriptorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MedEasy.Objects.PrescriptionItem", b =>
                {
                    b.HasOne("MedEasy.Objects.Prescription")
                        .WithMany("Items")
                        .HasForeignKey("PrescriptionId");
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

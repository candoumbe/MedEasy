using AutoMapper;
using MedEasy.DTO;
using MedEasy.DTO.Autocomplete;
using MedEasy.Objects;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;

namespace MedEasy.Mapping
{
    public class AutoMapperConfig
    {
        public static MapperConfiguration Build() => new MapperConfiguration(cfg =>
        {

            cfg.CreateMap<IEntity<int>, ResourceBase<Guid>>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID));

            cfg.CreateMap<Patient, PatientInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .ForMember(dest => dest.MainDoctorId, opt => opt.MapFrom(source => source.MainDoctor.UUID))
                .IncludeBase<IEntity<int>, ResourceBase<Guid>>()
                .ReverseMap();
            cfg.CreateMap<CreatePatientInfo, Patient>()
                .ForMember(dest => dest.MainDoctorId, opt => opt.Ignore());

            cfg.CreateMap<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(source => source.Patient.UUID))
                .ReverseMap();

            cfg.CreateMap<CreateTemperatureInfo, Temperature>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore());

            cfg.CreateMap<Temperature, TemperatureInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap();

            cfg.CreateMap<CreateBloodPressureInfo, BloodPressure>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore());
            cfg.CreateMap<BloodPressure, BloodPressureInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>();

            cfg.CreateMap<BodyWeight, BodyWeightInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>();

            cfg.CreateMap<DocumentMetadata, DocumentMetadataInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(source => source.Patient.UUID))
                .ForMember(dest => dest.DocumentId, opt => opt.MapFrom(source => source.Document.UUID))
                .IncludeBase<IEntity<int>, ResourceBase<Guid>>()
                .ReverseMap();
            cfg.CreateMap<Doctor, DoctorInfo>()
                .ForMember(dest => dest.SpecialtyId, opt => opt.MapFrom(source => source.Specialty.UUID))
                .IncludeBase<IEntity<int>, ResourceBase<Guid>>();

            cfg.CreateMap<Specialty, SpecialtyInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<IEntity<int>, ResourceBase<Guid>>()
                .ReverseMap();
            cfg.CreateMap<CreateSpecialtyInfo, Specialty>();

            cfg.CreateMap<Prescription, PrescriptionHeaderInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(source => source.Patient.UUID))
                .ForMember(dest => dest.PrescriptorId, opt => opt.MapFrom(source => source.Prescriptor.UUID))
                .IncludeBase<IEntity<int>, ResourceBase<Guid>>();

            cfg.CreateMap<CreatePrescriptionInfo, Prescription>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.PrescriptorId, opt => opt.Ignore());

            cfg.CreateMap<Prescription, PrescriptionInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(source => source.Patient.UUID))
                .IncludeBase<IEntity<int>, ResourceBase<Guid>>()
                .ReverseMap();
            cfg.CreateMap<PrescriptionItem, PrescriptionItemInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<IEntity<int>, ResourceBase<Guid>>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());


            cfg.CreateMap<Appointment, AppointmentInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(source => source.Patient.UUID))
                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(source => source.Doctor.UUID))
                .IncludeBase<IEntity<int>, ResourceBase<Guid>>()
                .ReverseMap();

            #region Autocomplete

            cfg.CreateMap<Doctor, DoctorAutocompleteInfo>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(source => source.UUID))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(source => source.Firstname + " " + source.Lastname))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(source => source.Specialty.Name));


            #endregion

            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));

        });
    }
}

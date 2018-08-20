using AutoMapper;
using Measures.DTO;
using Measures.Objects;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;

namespace Measures.Mapping
{
    /// <summary>
    /// Contains mappings configuration
    /// </summary>
    public static class AutoMapperConfig
    {
        /// <summary>
        /// Creates a new <see cref="MapperConfiguration"/>
        /// </summary>
        /// <returns></returns>
        public static MapperConfiguration Build() => new MapperConfiguration(cfg =>
        {
            cfg.CreateCoreMapping();
            cfg.CreateMap<NewPatientInfo, Patient>()
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                .ForMember(entity => entity.CreatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.CreatedDate, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedDate, opt => opt.Ignore())
                .ForMember(entity => entity.UUID, opt => opt.Ignore())
                ;

            cfg.CreateMap<Patient, PatientInfo>()
                .ForMember(dest => dest.Fullname, opt => opt.Ignore())
                .IncludeBase<IEntity<int>, Resource<Guid>>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                ;

            
            cfg.CreateMap<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.UUID))
                .ForMember(dto => dto.PatientId, opt => opt.MapFrom(entity => entity.Patient.UUID))
                .ReverseMap()
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                .ForMember(entity => entity.UUID, opt => opt.MapFrom(dto => dto.Id))
                .ForMember(entity => entity.Patient, opt => opt.Ignore())
                .ForMember(entity => entity.PatientId, opt => opt.Ignore())
                ;

            cfg.CreateMap<CreateBloodPressureInfo, BloodPressure>()
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                .ForMember(entity => entity.UUID, opt => opt.Ignore())
                .ForMember(entity => entity.Patient, opt => opt.Ignore())
                .ForMember(entity => entity.PatientId, opt => opt.Ignore())
                .ForMember(entity => entity.CreatedDate, opt => opt.Ignore())
                .ForMember(entity => entity.CreatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedDate, opt => opt.Ignore());

            cfg.CreateMap<BloodPressure, BloodPressureInfo>()
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap()
                ;

            cfg.CreateMap<CreateTemperatureInfo, Temperature>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UUID, opt => opt.ResolveUsing((source, dest) => Guid.NewGuid()))
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());

            cfg.CreateMap<Temperature, TemperatureInfo>()
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap<BodyWeight, BodyWeightInfo>()
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));


        });
    }
}

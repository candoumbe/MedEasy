using AutoMapper;
using Measures.DTO;
using Measures.Objects;
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
    public class AutoMapperConfig
    {
        /// <summary>
        /// Creates a new <see cref="MapperConfiguration"/>
        /// </summary>
        /// <returns></returns>
        public static MapperConfiguration Build() => new MapperConfiguration(cfg =>
        {

            cfg.CreateMap<IEntity<int>, Resource<Guid>>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            cfg.CreateMap<Patient, PatientInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
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
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(source => source.Patient.UUID))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                ;

            cfg.CreateMap<CreateBloodPressureInfo, BloodPressure>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UUID, opt => opt.Ignore())
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());

            cfg.CreateMap<BloodPressure, BloodPressureInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap<CreateTemperatureInfo, Temperature>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UUID, opt => opt.ResolveUsing((source, dest) => Guid.NewGuid()))
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());

            cfg.CreateMap<Temperature, TemperatureInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap<BodyWeight, BodyWeightInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));


        });
    }
}

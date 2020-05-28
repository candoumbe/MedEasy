using AutoMapper;
using Measures.DTO;
using Measures.Objects;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;
using System.Linq;
using static System.StringSplitOptions;

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
            
            cfg.CreateMap<Patient, PatientInfo>()
                .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
                .IncludeBase<IEntity<Guid>, Resource<Guid>>();

            cfg.CreateMap<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ForMember(dto => dto.PatientId, opt => opt.MapFrom(entity => entity.PatientId))
                .ReverseMap();

            cfg.CreateMap<BloodPressure, BloodPressureInfo>()
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap();

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

            cfg.CreateMap<GenericMeasure, GenericMeasureInfo>()
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));
        });
    }
}

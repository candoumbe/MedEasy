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

            cfg.CreateMap<Patient, PatientInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<IEntity<int>, Resource<Guid>>()
                .ReverseMap();
            
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
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap();

            cfg.CreateMap<BodyWeight, BodyWeightInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<PhysiologicalMeasurement, PhysiologicalMeasurementInfo>()
                .ReverseMap();
            
            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));
        });
    }
}

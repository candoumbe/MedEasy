namespace Measures.Mapping
{
    using AutoMapper;

    using Measures.DTO;
    using Measures.Ids;
    using Measures.Objects;

    using MedEasy.Mapping;
    using MedEasy.Objects;
    using MedEasy.RestObjects;

    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.JsonPatch.Operations;

    /// <summary>
    /// Contains mappings configuration
    /// </summary>
    public static class AutoMapperConfig
    {
        /// <summary>
        /// Creates a new <see cref="MapperConfiguration"/>
        /// </summary>
        /// <returns></returns>
        public static MapperConfiguration Build() => new(cfg =>
        {
            cfg.CreateCoreMapping();

            cfg.CreateMap<Subject, SubjectInfo>()
                .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Name))
                .IncludeBase<IEntity<SubjectId>, Resource<SubjectId>>();

            cfg.CreateMap(typeof(PhysiologicalMeasurement<>), typeof(PhysiologicalMeasurementInfo<>))
                .ForMember(nameof(PhysiologicalMeasurementInfo<BloodPressureId>.SubjectId),
                           opt => opt.MapFrom(nameof(PhysiologicalMeasurement<BloodPressureId>.SubjectId)))
                .ReverseMap();

            cfg.CreateMap<BloodPressure, BloodPressureInfo>()
                .IncludeBase<PhysiologicalMeasurement<BloodPressureId>, PhysiologicalMeasurementInfo<BloodPressureId>>()
                .ReverseMap();

            cfg.CreateMap<Temperature, TemperatureInfo>()
                .IncludeBase<PhysiologicalMeasurement<TemperatureId>, PhysiologicalMeasurementInfo<TemperatureId>>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap<BodyWeight, BodyWeightInfo>()
                .IncludeBase<PhysiologicalMeasurement<BodyWeightId>, PhysiologicalMeasurementInfo<BodyWeightId>>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));
        });
    }
}

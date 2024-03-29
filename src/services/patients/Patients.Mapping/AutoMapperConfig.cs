﻿namespace Patients.Mapping
{
    using AutoMapper;

    using MedEasy.Mapping;
    using MedEasy.Objects;
    using MedEasy.RestObjects;

    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.JsonPatch.Operations;

    using Patients.DTO;
    using Patients.Ids;
    using Patients.Objects;

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

            cfg.CreateMap<Patient, PatientInfo>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Id))
                .ForMember(dto => dto.MainDoctorId, opt => opt.Ignore())
                .IncludeBase<IEntity<PatientId>, Resource<PatientId>>()
                .ReverseMap()
                .ForMember(entity => entity.CreatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedBy, opt => opt.Ignore())
                ;

            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));
        });
    }
}

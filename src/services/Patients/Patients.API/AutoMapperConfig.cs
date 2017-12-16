using AutoMapper;
using MedEasy.Objects;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Patients.DTO;
using Patients.Objects;
using System;

namespace Patients.Mapping
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

            cfg.CreateMap<CreatePatientInfo, Patient>()
                .ForMember(dest => dest.UUID, opt => opt.MapFrom(source => source.Id))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .IncludeBase<Resource<Guid>, IEntity<int>>()
                .ReverseMap();

            cfg.CreateMap<Patient, PatientInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .IncludeBase<IEntity<int>, Resource<Guid>>()
                .ReverseMap();
            
            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));
        });
    }
}

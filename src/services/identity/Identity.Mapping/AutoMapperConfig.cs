using AutoMapper;
using Identity.DTO;
using Identity.Objects;
using MedEasy.Objects;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;

namespace Identity.Mapping
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

            cfg.CreateMap<Account, AccountInfo>();

            cfg.CreateMap<Claim, ClaimInfo>();

            
            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));


        });
    }
}

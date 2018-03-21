using AutoMapper;
using MedEasy.Objects;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Mapping
{
    public static class AutoMapperConfig
    {
        public static void CreateCoreMapping(this IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<IEntity<int>, Resource<Guid>>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.UUID))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}

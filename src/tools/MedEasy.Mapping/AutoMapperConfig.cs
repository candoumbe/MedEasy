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
            cfg.CreateMap<IEntity<Guid>, Resource<Guid>>()
                .ForMember(dto => dto.CreatedDate, opt => opt.Ignore())
                .ForMember(dto => dto.UpdatedDate, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap(typeof(AuditableBaseEntity<>), typeof(Resource<>))
                .ReverseMap();

            cfg.CreateMap(typeof(AuditableEntity<,>), typeof(Resource<>))
               .ReverseMap();
        }
    }
}

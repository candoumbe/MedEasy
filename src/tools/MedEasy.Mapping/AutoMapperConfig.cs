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
            cfg.CreateMap(typeof(IEntity<>), typeof(Resource<>))
                .ForMember(nameof(Resource<Guid>.CreatedDate), opt => opt.Ignore())
                .ForMember(nameof(Resource<Guid>.UpdatedDate), opt => opt.Ignore())
                .ReverseMap()
                .ForMember(nameof(IEntity<Guid>.Id), opt => opt.Ignore())
                ;

            cfg.CreateMap(typeof(AuditableBaseEntity<>), typeof(Resource<>))
                .ForMember(nameof(Resource<Guid>.CreatedDate), opt => opt.MapFrom(nameof(AuditableBaseEntity<Guid>.CreatedDate)))
                .ForMember(nameof(Resource<Guid>.UpdatedDate), opt => opt.MapFrom(nameof(AuditableBaseEntity<Guid>.UpdatedDate)))
                .ReverseMap();

            cfg.CreateMap(typeof(AuditableEntity<,>), typeof(Resource<>))
               .ReverseMap();

            cfg.CreateMap(typeof(Entity<,>), typeof(Resource<>))
               .ForMember(nameof(Resource<Guid>.CreatedDate), opt => opt.Ignore())
               .ForMember(nameof(Resource<Guid>.UpdatedDate), opt => opt.Ignore())
               .ReverseMap();
        }
    }
}

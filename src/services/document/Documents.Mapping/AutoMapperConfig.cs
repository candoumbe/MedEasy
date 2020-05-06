using AutoMapper;

using Documents.DTO.v1;
using Documents.Objects;

using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.RestObjects;

using System;

namespace Documents.Mapping
{
    public static class AutoMapperConfig
    {
        /// <summary>
        /// Builds mappings between entities and dtos
        /// </summary>
        /// <returns></returns>
        public static MapperConfiguration Build() => new MapperConfiguration((cfg) =>
        {
            cfg.CreateCoreMapping();
            cfg.CreateMap<Document, DocumentInfo>()
                .ForMember(dto => dto.CreatedDate, opt => opt.MapFrom(entity => entity.CreatedDate))
                .ForMember(dto => dto.UpdatedDate, opt => opt.MapFrom(entity => entity.UpdatedDate))
                .IncludeBase<IEntity<Guid>, Resource<Guid>>();

            cfg.CreateMap<Document, DocumentFileInfo>()
                .ForMember(dto => dto.Content, opt => opt.MapFrom(entity => entity.File.Content));
                
        });
    }
}

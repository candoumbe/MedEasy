using AutoMapper;

using Documents.DTO.v1;
using Documents.Ids;
using Documents.Objects;

using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.RestObjects;

namespace Documents.Mapping
{
    public static class AutoMapperConfig
    {
        /// <summary>
        /// Builds mappings between entities and dtos
        /// </summary>
        /// <returns></returns>
        public static MapperConfiguration Build() => new((cfg) =>
        {
            cfg.CreateCoreMapping();
            cfg.CreateMap<Document, DocumentInfo>()
                .ForMember(dto => dto.CreatedDate, opt => opt.MapFrom(entity => entity.CreatedDate))
                .ForMember(dto => dto.UpdatedDate, opt => opt.MapFrom(entity => entity.UpdatedDate))
                .IncludeBase<IEntity<DocumentId>, Resource<DocumentId>>();

            cfg.CreateMap<DocumentPart, DocumentPartInfo>()
                .ForMember(dto => dto.Position, opt => opt.MapFrom(entity => entity.Position))
                .ForMember(dto => dto.Size, opt => opt.MapFrom(entity => entity.Size));

        });
    }
}

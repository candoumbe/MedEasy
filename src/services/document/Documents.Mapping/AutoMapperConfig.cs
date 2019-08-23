using AutoMapper;
using Documents.DTO.v1;
using Documents.Objects;
using MedEasy.Mapping;

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
                .IgnoreAllPropertiesWithAnInaccessibleSetter();
                
        });
    }
}

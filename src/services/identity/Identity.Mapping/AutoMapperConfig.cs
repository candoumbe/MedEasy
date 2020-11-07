using AutoMapper;

using Identity.DTO;
using Identity.Objects;

using MedEasy.Mapping;
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
    public static class AutoMapperConfig
    {
        /// <summary>
        /// Creates a new <see cref="MapperConfiguration"/>
        /// </summary>
        /// <returns></returns>
        public static MapperConfiguration Build() => new MapperConfiguration(cfg =>
        {
            cfg.CreateCoreMapping();
            cfg.CreateMap<Account, AccountInfo>()
                .IncludeBase<AuditableEntity<Guid, Account>, Resource<Guid>>()
                .ReverseMap()
                .ForMember(entity => entity.Claims, opt => opt.Ignore() );

            cfg.CreateMap<Account, SearchAccountInfoResult>()
               .IncludeBase<IEntity<Guid>, Resource<Guid>>();

            cfg.CreateMap<AccountClaim, ClaimInfo>()
                .ForMember(dto => dto.Type, opt => opt.MapFrom(entity => entity.Claim.Type))
                .ForMember(dto => dto.Value, opt => opt.MapFrom(entity => entity.Claim.Value));

            cfg.CreateMap<NewAccountInfo, Account>()
                .ConstructUsing(dto => new Account(id: Guid.NewGuid(),
                                          username: dto.Username,
                                          tenantId: dto.TenantId,
                                          name: dto.Name,
                                          email: dto.Email,
                                          passwordHash: null,
                                          salt: null)
                )
                .ForMember(entity => entity.Salt, opt => opt.Ignore())
                .ForMember(entity => entity.PasswordHash, opt => opt.Ignore())
                .ForMember(entity => entity.EmailConfirmed, opt => opt.Ignore())
                .ForMember(entity => entity.Locked, opt => opt.Ignore())
                .ForMember(entity => entity.CreatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.CreatedDate, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedDate, opt => opt.Ignore())
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                .ForMember(entity => entity.IsActive, opt => opt.UseValue(false))
                .ForMember(entity => entity.RefreshToken, opt => opt.Ignore())
                ;

            cfg.CreateMap<Claim, ClaimInfo>();

            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));
        });
    }
}

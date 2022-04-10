namespace Identity.Mapping
{
    using AutoMapper;

    using Identity.DTO;
    using Identity.Ids;
    using Identity.Objects;
    using Identity.ValueObjects;

    using MedEasy.Mapping;
    using MedEasy.Objects;
    using MedEasy.RestObjects;

    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.JsonPatch.Operations;

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
            cfg.CreateMap<IEntity<AccountId>, Resource<AccountId>>()
                .ForMember(dto => dto.CreatedDate, opt => opt.Ignore())
                .ForMember(dto => dto.UpdatedDate, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap<Account, AccountInfo>()
                .IncludeBase<AuditableEntity<AccountId, Account>, Resource<AccountId>>()
                .ReverseMap()
                .ForMember(entity => entity.Claims, opt => opt.Ignore());

            cfg.CreateMap<AccountRole, RoleInfo>()
               .ForMember(info => info.Name, opt => opt.MapFrom(entity => entity.Role.Code))
               .ForMember(info => info.Claims, opt => opt.MapFrom(entity => entity.Role.Claims));

            cfg.CreateMap<Account, SearchAccountInfoResult>()
               .IncludeBase<IEntity<AccountId>, Resource<AccountId>>();

            cfg.CreateMap<AccountClaim, ClaimInfo>()
                .ForMember(dto => dto.Type, opt => opt.MapFrom(entity => entity.Claim.Type))
                .ForMember(dto => dto.Value, opt => opt.MapFrom(entity => entity.Claim.Value));

            cfg.CreateMap<NewAccountInfo, Account>()
                .ConstructUsing(dto => new Account(dto.Id,
                                                   dto.Username,
                                                   dto.Email,
                                                   null,
                                                   null,
                                                   dto.Name,
                                                   false,
                                                   false,
                                                   dto.TenantId,
                                                   null)
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
                .ForMember(entity => entity.IsActive, opt => opt.MapFrom(_ => false))
                .ForMember(entity => entity.RefreshToken, opt => opt.Ignore())
                .ForMember(entity => entity.Roles, opt => opt.Ignore())
                .ForMember(entity => entity.Claims, opt => opt.Ignore());

            cfg.CreateMap<Claim, ClaimInfo>();

            cfg.CreateMap<RoleClaim, ClaimInfo>()
               .ForMember(dto => dto.Type, opt => opt.MapFrom(entity => entity.Claim.Type))
               .ForMember(dto => dto.Value, opt => opt.MapFrom(entity => entity.Claim.Value));

            cfg.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            cfg.CreateMap(typeof(Operation<>), typeof(Operation<>));
        });
    }
}

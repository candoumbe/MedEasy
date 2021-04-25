using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Identity.Ids
{
    /// <summary>
    /// <see cref="RoleClaim"/>'s identifier
    /// </summary>
    public record RoleClaimId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        /// <summary>
        /// Creates a new <see cref="RoleClaimId"/>
        /// </summary>
        /// <returns>The newly created <see cref="AccountClaimId"/></returns>
        public static RoleClaimId New() => new(Guid.NewGuid());

        public static RoleClaimId Empty => new(Guid.Empty);

        public class EfValueConverter : ValueConverter<RoleClaimId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(id => id.Value,
                                                                                      value => new RoleClaimId(value), mappingHints)
            { }
        }
    }
}

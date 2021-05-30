namespace Identity.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// <see cref="RoleClaim"/>'s identifier
    /// </summary>
    [JsonConverter(typeof(StronglyTypedIdJsonConverter<RoleClaimId, Guid>))]
    public record RoleClaimId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        /// <summary>
        /// Creates a new <see cref="RoleClaimId"/>
        /// </summary>
        /// <returns>The newly created <see cref="AccountClaimId"/></returns>
        public static RoleClaimId New() => new(Guid.NewGuid());

        public static RoleClaimId Empty => new(Guid.Empty);

#pragma warning disable S1185 // Overriding members should do more than simply call the same member in the base class
        public override string ToString() => base.ToString();
#pragma warning restore S1185 // Overriding members should do more than simply call the same member in the base class


        public class EfValueConverter : ValueConverter<RoleClaimId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(id => id.Value,
                                                                                      value => new RoleClaimId(value), mappingHints)
            { }
        }
    }
}

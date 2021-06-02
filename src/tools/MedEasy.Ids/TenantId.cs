namespace MedEasy.Ids
{
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Identifier for thent
    /// </summary>
    [JsonConverter(typeof(StronglyTypedIdJsonConverter<TenantId, Guid>))]
    public record TenantId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static TenantId New() => new(Guid.NewGuid());

        public static TenantId Empty => new(Guid.Empty);

#pragma warning disable S1185 // Overriding members should do more than simply call the same member in the base class
        public override string ToString() => base.ToString();
#pragma warning restore S1185 // Overriding members should do more than simply call the same member in the base class

        public class EfValueConverter : ValueConverter<TenantId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new TenantId(value),
                mappingHints
            )
            { }
        }
    }
}

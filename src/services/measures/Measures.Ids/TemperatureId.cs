namespace Measures.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(StronglyTypedIdJsonConverter<TemperatureId, Guid>))]
    public record TemperatureId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static TemperatureId New() => new(Guid.NewGuid());
        public static TemperatureId Empty => new(Guid.Empty);

        public override string ToString() => base.ToString();

        public class EfValueConverter : ValueConverter<TemperatureId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new TemperatureId(value), mappingHints) { }
        }
    }
}

namespace Measures.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(StronglyTypedIdJsonConverter<BodyWeightId, Guid>))]
    public record BodyWeightId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static BodyWeightId New() => new(Guid.NewGuid());
        public static BodyWeightId Empty => new(Guid.Empty);

        public override string ToString() => base.ToString();

        public class EfValueConverter : ValueConverter<BodyWeightId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new BodyWeightId(value), mappingHints) { }
        }
    }
}

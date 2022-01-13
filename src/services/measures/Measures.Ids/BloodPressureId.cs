namespace Measures.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
using System.Text.Json.Serialization;

    [JsonConverter(typeof(StronglyTypedIdJsonConverter<BloodPressureId, Guid>))]
    public record BloodPressureId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static BloodPressureId New() => new(Guid.NewGuid());
        public static BloodPressureId Empty => new(Guid.Empty);

        public override string ToString() => base.ToString();

        public class EfValueConverter : ValueConverter<BloodPressureId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new BloodPressureId(value), mappingHints) { }
        }
    }
}

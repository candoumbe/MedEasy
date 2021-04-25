using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Measures.Ids
{
    public record TemperatureId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static TemperatureId New() => new(Guid.NewGuid());
        public static TemperatureId Empty => new(Guid.Empty);

        public class EfValueConverter : ValueConverter<TemperatureId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new TemperatureId(value), mappingHints) { }
        }
    }
}

using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Measures.Ids
{
    public record BodyWeightId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static BodyWeightId New() => new(Guid.NewGuid());
        public static BodyWeightId Empty => new(Guid.Empty);

        public class EfValueConverter : ValueConverter<BodyWeightId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new BodyWeightId(value), mappingHints) { }
        }
    }
}

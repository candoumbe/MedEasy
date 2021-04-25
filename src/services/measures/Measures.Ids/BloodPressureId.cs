using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Measures.Ids
{
    public record BloodPressureId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static BloodPressureId New() => new(Guid.NewGuid());
        public static BloodPressureId Empty => new(Guid.Empty);

        public class EfValueConverter : ValueConverter<BloodPressureId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new BloodPressureId(value), mappingHints) { }
        }
    }
}

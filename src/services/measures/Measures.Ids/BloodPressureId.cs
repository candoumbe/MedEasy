namespace Measures.Ids
{
    using MedEasy.Ids;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;

    public record BloodPressureId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static BloodPressureId New() => new(Guid.NewGuid());
        public static BloodPressureId Empty => new(Guid.Empty);

#pragma warning disable S1185 // Overriding members should do more than simply call the same member in the base class
        public override string ToString() => base.ToString();
#pragma warning restore S1185 // Overriding members should do more than simply call the same member in the base class


        public class EfValueConverter : ValueConverter<BloodPressureId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new BloodPressureId(value), mappingHints) { }
        }
    }
}

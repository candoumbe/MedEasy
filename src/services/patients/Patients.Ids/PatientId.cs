namespace Patients.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
using System.Text.Json.Serialization;

    [JsonConverter(typeof(StronglyTypedIdJsonConverter<PatientId, Guid>))]
    public record PatientId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static PatientId Empty => new(Guid.Empty);
        public static PatientId New() => new(Guid.NewGuid());

#pragma warning disable S1185 // Overriding members should do more than simply call the same member in the base class
        public override string ToString() => base.ToString();
#pragma warning restore S1185 // Overriding members should do more than simply call the same member in the base class

        public class EfValueConverter : ValueConverter<PatientId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new PatientId(value), mappingHints) { }
        }
    }
}

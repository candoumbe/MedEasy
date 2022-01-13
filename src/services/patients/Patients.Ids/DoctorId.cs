namespace Patients.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(StronglyTypedIdJsonConverter<DoctorId, Guid>))]
    public record DoctorId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static DoctorId Empty => new(Guid.Empty);
        public static DoctorId New() => new(Guid.NewGuid());

        public override string ToString() => base.ToString();

        public class EfValueConverter : ValueConverter<DoctorId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new DoctorId(value),
                mappingHints
            )
            { }
        }
    }
}

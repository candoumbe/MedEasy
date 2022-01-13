namespace Agenda.Ids
{

    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(StronglyTypedIdJsonConverter<AppointmentId, Guid>))]
    public record AppointmentId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static AppointmentId New() => new(Guid.NewGuid());

        public static AppointmentId Empty => new(Guid.Empty);

        public override string ToString() => base.ToString();

        public class EfValueConverter : ValueConverter<AppointmentId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new AppointmentId(value),
                mappingHints
            )
            { }
        }
    }

}

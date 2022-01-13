namespace Agenda.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(StronglyTypedIdJsonConverter<AttendeeId, Guid>))]
    public record AttendeeId(Guid Value) : StronglyTypedId<Guid>(Value)
    {

        public static AttendeeId New() => new(Guid.NewGuid());

        public static AttendeeId Empty => new(Guid.Empty);

        public override string ToString() => base.ToString();

        public class EfValueConverter : ValueConverter<AttendeeId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new AttendeeId(value),
                mappingHints
            )
            { }
        }
    }

}

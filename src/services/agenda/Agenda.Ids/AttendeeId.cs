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

#pragma warning disable S1185 // Overriding members should do more than simply call the same member in the base class
        public override string ToString() => base.ToString();
#pragma warning restore S1185 // Overriding members should do more than simply call the same member in the base class

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

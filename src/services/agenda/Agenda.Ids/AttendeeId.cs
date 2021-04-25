using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Agenda.Ids
{
    public record AttendeeId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static AttendeeId New() => new(Guid.NewGuid());

        public static AttendeeId Empty => new(Guid.Empty);

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


using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Agenda.Ids
{
    public record AppointmentId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static AppointmentId New() => new(Guid.NewGuid());

        public static AppointmentId Empty => new(Guid.Empty);

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

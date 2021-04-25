using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Patients.Ids
{
    public record DoctorId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static DoctorId Empty => new(Guid.Empty);
        public static DoctorId New() => new(Guid.NewGuid());

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

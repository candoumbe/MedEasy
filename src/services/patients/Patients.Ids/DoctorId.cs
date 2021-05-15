namespace Patients.Ids
{
    using MedEasy.Ids;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;

    public record DoctorId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static DoctorId Empty => new(Guid.Empty);
        public static DoctorId New() => new(Guid.NewGuid());

#pragma warning disable S1185 // Overriding members should do more than simply call the same member in the base class
        public override string ToString() => base.ToString();
#pragma warning restore S1185 // Overriding members should do more than simply call the same member in the base class

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

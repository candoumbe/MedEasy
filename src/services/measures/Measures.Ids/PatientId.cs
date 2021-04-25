using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Measures.Ids
{
    public record PatientId(Guid Value) : StronglyTypedGuidId(Value)
    {
        public static PatientId New() => new(Guid.NewGuid());

        public static PatientId Empty => new(Guid.Empty);

        public class EfValueConverter : ValueConverter<PatientId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new PatientId(value), mappingHints) { }
        }
    }
}

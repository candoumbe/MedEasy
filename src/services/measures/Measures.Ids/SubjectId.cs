namespace Measures.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(StronglyTypedIdJsonConverter<SubjectId, Guid>))]
    public record SubjectId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static SubjectId New() => new(Guid.NewGuid());

        public static SubjectId Empty => new(Guid.Empty);

#pragma warning disable S1185 // Overriding members should do more than simply call the same member in the base class
        public override string ToString() => base.ToString();
#pragma warning restore S1185 // Overriding members should do more than simply call the same member in the base class

        public class EfValueConverter : ValueConverter<SubjectId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new SubjectId(value), mappingHints) { }
        }
    }
}

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

        public override string ToString() => base.ToString();

        public class EfValueConverter : ValueConverter<SubjectId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new SubjectId(value), mappingHints) { }
        }
    }
}

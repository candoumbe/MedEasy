namespace Documents.Ids
{
    using MedEasy.Ids;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;

    public record DocumentId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static DocumentId New() => new(Guid.NewGuid());

        public static DocumentId Empty => new(Guid.Empty);


        public class EfValueConverter : ValueConverter<DocumentId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new DocumentId(value),
                mappingHints
            )
            { }
        }
    }
}

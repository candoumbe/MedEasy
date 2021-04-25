using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace MedEasy.Ids
{
    /// <summary>
    /// Identifier for thent
    /// </summary>
    public record TenantId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        public static TenantId New() => new(Guid.NewGuid());

        public static TenantId Empty => new(Guid.Empty);

        public class EfValueConverter : ValueConverter<TenantId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new TenantId(value),
                mappingHints
            )
            { }
        }
    }
}

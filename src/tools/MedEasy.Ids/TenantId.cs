namespace MedEasy.Ids
{
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Identifier for thent
    /// </summary>
    [JsonConverter(typeof(StronglyTypedIdJsonConverter<TenantId, Guid>))]
    public record TenantId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        /// <summary>
        /// Creates a new <see cref="TenantId"/> value
        /// </summary>
        /// <returns>a new a unique <see cref="TenantId"/>.</returns>
        public static TenantId New() => new(Guid.NewGuid());

        /// <summary>
        /// Value to use for whenever a value for <see cref="TenantId"/> is needed and none can be provided.
        /// </summary>
        public static TenantId Empty => new(Guid.Empty);

        ///<inheritdoc/>
        public override string ToString() => base.ToString();

        /// <summary>
        /// EF Value converter for <see cref="TenantId"/>
        /// </summary>
        public class EfValueConverter : ValueConverter<TenantId, Guid>
        {
            /// <summary>
            /// Builds a new <see cref="EfValueConverter"/> to handle conversion from/to <see cref="TenantId"/>.
            /// </summary>
            /// <param name="mappingHints"></param>
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new TenantId(value),
                mappingHints
            )
            { }
        }
    }
}

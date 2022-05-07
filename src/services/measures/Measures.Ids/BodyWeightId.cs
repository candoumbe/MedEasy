namespace Measures.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// An identifier of a body weight measure.
    /// </summary>
    /// <param name="Value"></param>
    [JsonConverter(typeof(StronglyTypedIdJsonConverter<BodyWeightId, Guid>))]
    public record BodyWeightId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        /// <summary>
        /// Creates a new <see cref="BodyWeightId"/> instance
        /// </summary>
        /// <returns></returns>
        public static BodyWeightId New() => new(Guid.NewGuid());
        
        /// <summary>
        /// Creates a <see cref="BodyWeightId"/> instance that is empty
        /// </summary>
        public static BodyWeightId Empty => new(Guid.Empty);

        ///<inheritdoc/>
        public override string ToString() => base.ToString();

        public class EfValueConverter : ValueConverter<BodyWeightId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new BodyWeightId(value), mappingHints) { }
        }
    }
}

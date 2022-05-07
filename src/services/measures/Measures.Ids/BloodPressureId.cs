namespace Measures.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Identifies a blood pressure measurement.
    /// </summary>
    /// <param name="Value"></param>
    [JsonConverter(typeof(StronglyTypedIdJsonConverter<BloodPressureId, Guid>))]
    public record BloodPressureId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BloodPressureId"/> class.
        /// </summary>
        /// <returns></returns>
        public static BloodPressureId New() => new(Guid.NewGuid());

        /// <summary>
        /// Creates an 
        /// </summary>
        public static BloodPressureId Empty => new(Guid.Empty);

        ///<inheritdoc/>
        public override string ToString() => base.ToString();

        public class EfValueConverter : ValueConverter<BloodPressureId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null)
                : base(id => id.Value, value => new BloodPressureId(value), mappingHints) { }
        }
    }
}

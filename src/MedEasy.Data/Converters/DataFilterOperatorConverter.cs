using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.Data.Converters
{
    /// <summary>
    /// <see cref="JsonConverter"/> that support converting from/to <see cref="DataFilter"/>
    /// </summary>
    public class DataFilterOperatorConverter : JsonConverter
    {
        /// <summary>
        /// Defines matches between <see cref="DataFilterOperator"/> and strings
        /// </summary>
        private static IEnumerable<Tuple<DataFilterOperator, string>> AllowedOperators => new[]
        {
            Tuple.Create(DataFilterOperator.Contains, "contains"),
            Tuple.Create(DataFilterOperator.EndsWith, "endswith"),
            Tuple.Create(DataFilterOperator.EqualTo, "eq"),
            Tuple.Create(DataFilterOperator.GreaterThan, "gt"),
            Tuple.Create(DataFilterOperator.GreaterThanOrEqual, "gte"),
            Tuple.Create(DataFilterOperator.IsEmpty, "isempty"),
            Tuple.Create(DataFilterOperator.IsNotEmpty, "isnotempty"),
            Tuple.Create(DataFilterOperator.IsNotNull, "isnotnull"),
            Tuple.Create(DataFilterOperator.IsNull, "isnull"),
            Tuple.Create(DataFilterOperator.LessThan, "lt"),
            Tuple.Create(DataFilterOperator.LessThanOrEqualTo, "lte"),
            Tuple.Create(DataFilterOperator.NotEqualTo, "neq"),
            Tuple.Create(DataFilterOperator.StartsWith, "startswith")
        };

        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override bool CanConvert(Type type) => type == typeof(DataFilterOperator);

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DataFilterOperator kfo = DataFilterOperator.EqualTo;
            JToken token = JToken.ReadFrom(reader);
            if (token.Type == JTokenType.Property)
            {
                string value = token.Value<string>();
                kfo = ConvertFromStringToEnum(value);
            }

            return kfo;

        }

        /// <summary>
        /// Function that can convert a string to its <see cref="DataFilterOperator"/>
        /// </summary>
        private static Func<string, DataFilterOperator> ConvertFromStringToEnum => stringValue =>
        {
            Tuple<DataFilterOperator, string> tuple = AllowedOperators.SingleOrDefault(item => item.Item2 == stringValue);
            return tuple?.Item1 ?? DataFilterOperator.EqualTo;
        };

        /// <summary>
        /// Function that can convert a <see cref="DataFilterOperator"/> to its string representation
        /// </summary>
        private static Func<DataFilterOperator, string> ConvertFromEnumToString => enumValue =>
        {
            Tuple<DataFilterOperator, string> tuple = AllowedOperators.SingleOrDefault(item => item.Item1 == enumValue);
            return tuple?.Item2 ?? "eq";
        };

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DataFilterOperator @operator = (DataFilterOperator)value;

            writer.WritePropertyName(DataFilter.OperatorJsonPropertyName, true);
            writer.WriteValue($"{ConvertFromEnumToString(@operator)}");
        }
    }

}

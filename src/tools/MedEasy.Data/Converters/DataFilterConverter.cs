using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MedEasy.Data.Converters
{
    /// <summary>
    /// <see cref="JsonConvert"/> implementation that can convert from/to <see cref="DataFilter"/>
    /// </summary>
    public class DataFilterConverter : JsonConverter
    {
        private static IImmutableDictionary<string, DataFilterOperator> _operators = new Dictionary<string, DataFilterOperator>
        {
            ["contains"] = DataFilterOperator.Contains,
            ["endswith"] = DataFilterOperator.EndsWith,
            ["eq"] = DataFilterOperator.EqualTo,
            ["gt"] = DataFilterOperator.GreaterThan,
            ["gte"] = DataFilterOperator.GreaterThanOrEqual,
            ["isempty"] = DataFilterOperator.IsEmpty,
            ["isnotempty"] = DataFilterOperator.IsNotEmpty,
            ["isnotnull"] = DataFilterOperator.IsNotNull,
            ["isnull"] = DataFilterOperator.IsNull,
            ["lt"] = DataFilterOperator.LessThan,
            ["lte"] = DataFilterOperator.LessThanOrEqualTo,
            ["neq"] = DataFilterOperator.NotEqualTo,
            ["startswith"] = DataFilterOperator.StartsWith
        }.ToImmutableDictionary();

        public override bool CanConvert(Type objectType) => objectType == typeof(DataFilter);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DataFilter filter = null;

            JToken token = JToken.ReadFrom(reader);
            if (objectType == typeof(DataFilter))
            {
                if (token.Type == JTokenType.Object)
                {
                    IEnumerable<JProperty> properties = ((JObject)token).Properties();

                    if (properties.Any(prop => prop.Name == DataFilter.FieldJsonPropertyName)
                         && properties.Any(prop => prop.Name == DataFilter.OperatorJsonPropertyName))
                    {
                        string field = token[DataFilter.FieldJsonPropertyName].Value<string>();
                        DataFilterOperator @operator = _operators[token[DataFilter.OperatorJsonPropertyName].Value<string>()];
                        object value = null;
                        if (!DataFilter.UnaryOperators.Contains(@operator))
                        {
                            value = token[DataFilter.ValueJsonPropertyName]?.Value<string>();
                        }
                        filter = new DataFilter(field, @operator, value);
                    }
                }
            }

            return filter?.As(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DataFilter kf = (DataFilter)value;

            writer.WriteStartObject();

            // Field
            writer.WritePropertyName(DataFilter.FieldJsonPropertyName);
            writer.WriteValue(kf.Field);

            // operator
            writer.WritePropertyName(DataFilter.OperatorJsonPropertyName);
            KeyValuePair<string, DataFilterOperator> kv = _operators.Single(item => item.Value == kf.Operator);
            writer.WriteValue(kv.Key);

            // value (only if the operator is not an unary operator)
            if (!DataFilter.UnaryOperators.Contains(kf.Operator))
            {
                writer.WritePropertyName(DataFilter.ValueJsonPropertyName);
                writer.WriteValue(kf.Value);
            }

            writer.WriteEnd();
        }
    }
}

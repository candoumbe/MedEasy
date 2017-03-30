using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Data.Converters
{
    /// <summary>
    /// <see cref="JsonConvert"/> implementation that can convert from/to <see cref="DataFilter"/>
    /// </summary>
    public class DataFilterConverter : JsonConverter
    {

        private static IImmutableDictionary<string, DataFilterOperator> Operators = new Dictionary<string, DataFilterOperator>
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
                        filter = new DataFilter()
                        {
                            Field = token[DataFilter.FieldJsonPropertyName].Value<string>(),
                            Operator = Operators[token[DataFilter.OperatorJsonPropertyName].Value<string>()],
                            Value = token[DataFilter.ValueJsonPropertyName]?.Value<string>()
                        };
                    }
                }
            }

            return filter?.As(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DataFilter kf = (DataFilter)value;

            writer.WriteStartObject();

            writer.WritePropertyName(DataFilter.FieldJsonPropertyName);
            writer.WriteValue(kf.Field);

            writer.WritePropertyName(DataFilter.OperatorJsonPropertyName);
            KeyValuePair<string, DataFilterOperator> kv = Operators.SingleOrDefault(item => item.Value == kf.Operator);
            writer.WriteValue(Equals(default(KeyValuePair<string, DataFilterOperator>), kv)
                ? Operators.Single(item => item.Value == DataFilterOperator.EqualTo).Key
                : kv.Key);


            writer.WritePropertyName(DataFilter.ValueJsonPropertyName);
            if (kf.Value != null)
            {
                writer.WriteValue(kf.Value);
            }
            else
            {
                writer.WriteNull();
            }

            writer.WriteEnd();
        }
    }

}

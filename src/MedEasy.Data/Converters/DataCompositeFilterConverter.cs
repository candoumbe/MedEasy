using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MedEasy.Data.Converters
{
    /// <summary>
    /// <see cref="JsonConverter"/> implementation that allow to convert json string from/to <see cref="DataCompositeFilter"/>
    /// </summary>
    public class DataCompositeFilterConverter : JsonConverter
    {

        private static IImmutableDictionary<string, DataFilterLogic> Logics = new Dictionary<string, DataFilterLogic>
        {
            [DataFilterLogic.And.ToString().ToLower()] = DataFilterLogic.And,
            [DataFilterLogic.Or.ToString().ToLower()] = DataFilterLogic.Or
        }.ToImmutableDictionary();


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
            ["startswith"] = DataFilterOperator.StartsWith,

        }.ToImmutableDictionary();

        public override bool CanConvert(Type objectType) => objectType == typeof(DataCompositeFilter);


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            DataCompositeFilter kcf = null;

            JToken token = JToken.ReadFrom(reader);
            if (objectType == typeof(DataCompositeFilter))
            {
                if (token.Type == JTokenType.Object)
                {
                    IEnumerable<JProperty> properties = ((JObject)token).Properties();

                    JProperty logicProperty = properties
                        .SingleOrDefault(prop => prop.Name == DataCompositeFilter.LogicJsonPropertyName);

                    if (logicProperty != null)
                    {
                        JProperty filtersProperty = properties.SingleOrDefault(prop => prop.Name == DataCompositeFilter.FiltersJsonPropertyName);
                        if (filtersProperty != null
                            && filtersProperty.Type == JTokenType.Array)
                        {
                            JArray filtersArray = (JArray)token[DataCompositeFilter.FiltersJsonPropertyName];
                            int nbFilters = filtersArray.Count();
                            if (nbFilters > 2)
                            {
                                IList<IDataFilter> filters = new List<IDataFilter>(nbFilters);
                                foreach (var item in filtersArray)
                                {
                                    IDataFilter kf = (IDataFilter)item.ToObject<DataFilter>() ?? item.ToObject<DataCompositeFilter>();

                                    if (kf != null)
                                    {
                                        filters.Add(kf);
                                    }
                                }


                                if (filters.Count() >= 2)
                                {
                                    kcf = new DataCompositeFilter
                                    {
                                        Logic = Logics[token[DataCompositeFilter.LogicJsonPropertyName].Value<string>()],
                                        Filters = filters
                                    };
                                }
                            }

                        }
                    }
                }
            }



            return kcf?.As(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DataCompositeFilter kcf = (DataCompositeFilter)value;

            writer.WriteStartObject();

            // TODO Maybe can we rely on the serializer to handle the logic serialization ?
            writer.WritePropertyName(DataCompositeFilter.LogicJsonPropertyName);
            writer.WriteValue(kcf.Logic.ToString().ToLower());

            writer.WritePropertyName(DataCompositeFilter.FiltersJsonPropertyName);
            writer.WriteStartArray();
            foreach (IDataFilter filter in kcf.Filters)
            {
                serializer.Serialize(writer, filter);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using static Newtonsoft.Json.JsonConvert;
using static Newtonsoft.Json.DefaultValueHandling;
using static Newtonsoft.Json.Required;
using MedEasy.Data.Converters;
using System;

namespace MedEasy.Data
{

    /// <summary>
    /// An instance of this class holds a kendo filter
    /// </summary>
    [JsonObject]
    [JsonConverter(typeof(DataFilterConverter))]
    public class DataFilter : IDataFilter, IEquatable<DataFilter>
    {
        /// <summary>
        /// Name of the json property that holds the field name
        /// </summary>
        public const string FieldJsonPropertyName = "field";

        /// <summary>
        /// Name of the json property that holds the operator
        /// </summary>
        public const string OperatorJsonPropertyName = "operator";

        /// <summary>
        /// Name of the json property that holds the value
        /// </summary>
        public const string ValueJsonPropertyName = "value";

        /// <summary>
        /// Generates the <see cref="JSchema"/> for the specified <see cref="DataFilterOperator"/>.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static JSchema Schema(DataFilterOperator op)
        {
            JSchema schema;
            switch (op)
            {
                case DataFilterOperator.Contains:
                case DataFilterOperator.IsEmpty:
                case DataFilterOperator.IsNotEmpty:
                case DataFilterOperator.StartsWith:
                case DataFilterOperator.EndsWith:
                    schema = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [FieldJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                            [OperatorJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                            [ValueJsonPropertyName] = new JSchema { Type = JSchemaType.String }
                        },
                        Required = { FieldJsonPropertyName, OperatorJsonPropertyName }
                    };
                    break;
                case DataFilterOperator.IsNotNull:
                case DataFilterOperator.IsNull:
                    schema = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [FieldJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                            [OperatorJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                            [ValueJsonPropertyName] = new JSchema { Type = JSchemaType.None }
                        },
                        Required = { FieldJsonPropertyName, OperatorJsonPropertyName }
                    };
                    break;
                default:
                    schema = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [FieldJsonPropertyName] = new JSchema { Type = JSchemaType.String,  },
                            [OperatorJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                            [ValueJsonPropertyName] = new JSchema { Type = JSchemaType.String | JSchemaType.Number | JSchemaType.Integer | JSchemaType.Boolean }
                        },
                        Required = { FieldJsonPropertyName, OperatorJsonPropertyName }
                    };
                    break;

            }

            return schema;

        }

        /// <summary>
        /// Name of the field to filter
        /// </summary>
        [JsonProperty(FieldJsonPropertyName, Required = Always)]
        public string Field { get; set; }

        /// <summary>
        /// Operator to apply to the filter
        /// </summary>
        [JsonProperty(OperatorJsonPropertyName, Required = Always)]
        [JsonConverter(typeof(DataFilterOperatorConverter))]
        public DataFilterOperator Operator { get; set; }

        /// <summary>
        /// Value of the filter
        /// </summary>
        [JsonProperty(ValueJsonPropertyName,
            Required = AllowNull,
            DefaultValueHandling = IgnoreAndPopulate,
            NullValueHandling = NullValueHandling.Ignore)]
        public object Value { get; set; }

        public virtual string ToJson()
#if DEBUG
        => SerializeObject(this, Formatting.Indented);
#else
            => SerializeObject(this);
#endif

#if DEBUG
        public override string ToString() => ToJson();
#endif

        public bool Equals(DataFilter other)
            => other != null && 
            ((ReferenceEquals(other, this) || 
            (Equals(other.Field, Field) && Equals(other.Operator, Operator) && Equals(other.Value, Value))));


    }

}

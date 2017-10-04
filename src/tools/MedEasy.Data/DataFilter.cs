using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using static Newtonsoft.Json.JsonConvert;
using static Newtonsoft.Json.DefaultValueHandling;
using static Newtonsoft.Json.Required;
using MedEasy.Data.Converters;
using System;
using System.Collections.Generic;

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
        public const string OperatorJsonPropertyName = "op";

        /// <summary>
        /// Name of the json property that holds the value
        /// </summary>
        public const string ValueJsonPropertyName = "value";


        /// <summary>
        /// <see cref="DataFilterOperator"/>s that required <see cref="Value"/> to be set value to a non null value
        /// </summary>
        private static readonly IEnumerable<DataFilterOperator> _unaryOperators = new[]{
            DataFilterOperator.IsEmpty,
            DataFilterOperator.IsNotEmpty,
            DataFilterOperator.IsNotNull,
            DataFilterOperator.IsNull
            };

        /// <summary>
        /// <see cref="DataFilterOperator"/>s that required <see cref="Value"/> to be null.
        /// </summary>
        public static IEnumerable<DataFilterOperator> UnaryOperators => _unaryOperators;


        /// <summary>
        /// Builds a new <see cref="DataFilter"/> instance.
        /// </summary>
        /// <param name="field">name of the field</param>
        /// <param name="operator"><see cref="DataFilter"/> to apply</param>
        /// <param name="value">value of the filter</param>
        public DataFilter(string field, DataFilterOperator @operator, object value = null)
        {
            Field = field;
            if (@operator == DataFilterOperator.EqualTo && value == null)
            {
                Operator = DataFilterOperator.IsNull;
            }
            else
            {
                Operator = @operator;
                Value = value;
            }
        }

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
                case DataFilterOperator.IsEmpty:
                case DataFilterOperator.IsNotEmpty:
                case DataFilterOperator.IsNotNull:
                case DataFilterOperator.IsNull:
                    schema = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [FieldJsonPropertyName] = new JSchema { Type = JSchemaType.String },
                            [OperatorJsonPropertyName] = new JSchema { Type = JSchemaType.String }
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
                            [ValueJsonPropertyName] = new JSchema {
                                Not = new JSchema() { Type = JSchemaType.Null }
                            }
                        },
                        Required = { FieldJsonPropertyName, OperatorJsonPropertyName, ValueJsonPropertyName }
                    };
                    break;

            }
            schema.AllowAdditionalProperties = false;

            return schema;

        }

        /// <summary>
        /// Name of the field to filter
        /// </summary>
        [JsonProperty(FieldJsonPropertyName, Required = Always)]
        public string Field { get; }

        /// <summary>
        /// Operator to apply to the filter
        /// </summary>
        [JsonProperty(OperatorJsonPropertyName, Required = Always)]
        //[JsonConverter(typeof(DataFilterOperatorConverter))]
        public DataFilterOperator Operator { get; }

        /// <summary>
        /// Value of the filter
        /// </summary>
        [JsonProperty(ValueJsonPropertyName,
            Required = AllowNull,
            DefaultValueHandling = IgnoreAndPopulate,
            NullValueHandling = NullValueHandling.Ignore)]
        public object Value { get; }

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

using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.Data.DataFilterOperator;

namespace MedEasy.Data.Tests.Converters
{
    public class DataFilterConverterTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        
        private static IImmutableDictionary<string, DataFilterOperator> Operators => new Dictionary<string, DataFilterOperator>
        {
            ["eq"] = EqualTo,
            ["neq"] = NotEqualTo,
            ["lt"] = LessThan,
            ["gt"] = GreaterThan,
            ["lte"] = LessThanOrEqualTo,
            ["gte"] = GreaterThanOrEqual,
            ["contains"] = Contains,
            ["isnull"] = IsNull,
            ["isnotnull"] = IsNotNull,
            ["isnotempty"] = IsNotEmpty,
            ["isempty"] = IsEmpty
        }.ToImmutableDictionary();

        public DataFilterConverterTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }


        /// <summary>
        /// Deserialize tests cases
        /// </summary>
        public static IEnumerable<object[]> DeserializeCases
        {
            get
            {
                foreach (KeyValuePair<string, DataFilterOperator> item in Operators.Where(op => op.Value != IsNull && op.Value != IsNotNull && op.Value != IsEmpty && op.Value != IsNotEmpty)  )
                {
                    yield return new object[]{
                        $"{{{DataFilter.FieldJsonPropertyName} :'Firstname', {DataFilter.OperatorJsonPropertyName} :'{item.Key}', {DataFilter.ValueJsonPropertyName} : 'Bruce'}}",
                        typeof(DataFilter),
                        ((Expression<Func<object, bool>>) ((result) => result is DataFilter
                            && "Firstname" == ((DataFilter)result).Field
                            && item.Value == ((DataFilter)result).Operator
                            && ((DataFilter)result).Value is string
                            && "Bruce".Equals((string)((DataFilter)result).Value)))
                    };
                }

                yield return new object[]{
                        $"{{{DataFilter.FieldJsonPropertyName} :'Firstname', {DataFilter.OperatorJsonPropertyName} :'isnull'}}",
                        typeof(DataFilter),
                        ((Expression<Func<object, bool>>) ((result) => result is DataFilter
                            && "Firstname" == ((DataFilter)result).Field
                            && Operators["isnull"] == ((DataFilter)result).Operator
                            && ((DataFilter)result).Value == null))
                    };


                yield return new object[]{
                        $"{{{DataFilter.FieldJsonPropertyName} :'Firstname', {DataFilter.OperatorJsonPropertyName} :'isnull', {DataFilter.ValueJsonPropertyName} : 6}}",
                        typeof(DataFilter),
                        ((Expression<Func<object, bool>>) ((result) => result is DataFilter
                            && "Firstname" == ((DataFilter)result).Field
                            && Operators["isnull"] == ((DataFilter)result).Operator
                            && ((DataFilter)result).Value == null))
                    };

                yield return new object[]{
                        $"{{{DataFilter.FieldJsonPropertyName}:'Firstname', {DataFilter.OperatorJsonPropertyName} :'isnull', value : null}}",
                        typeof(DataFilter),
                        ((Expression<Func<object, bool>>) ((result) => result is DataFilter
                            && "Firstname" == ((DataFilter)result).Field
                            && Operators["isnull"] == ((DataFilter)result).Operator
                            && ((DataFilter)result).Value == null))
                    };

                yield return new object[]{
                        $"{{{DataFilter.FieldJsonPropertyName}:'Firstname', {DataFilter.OperatorJsonPropertyName} :'isnull', value : null}}",
                        typeof(DataFilter),
                        ((Expression<Func<object, bool>>) ((result) => result is DataFilter
                            && "Firstname" == ((DataFilter)result).Field
                            && Operators["isnull"] == ((DataFilter)result).Operator
                            && ((DataFilter)result).Value == null))
                    };

                yield return new object[]{
                        $"{{{DataFilter.FieldJsonPropertyName} :'Firstname', {DataFilter.OperatorJsonPropertyName} :'isnotnull', value : null}}",
                        typeof(DataFilter),
                        ((Expression<Func<object, bool>>) ((result) => result is DataFilter
                            && "Firstname" == ((DataFilter)result).Field
                            && Operators["isnotnull"] == ((DataFilter)result).Operator
                            && ((DataFilter)result).Value == null))
                    };

                yield return new object[]{
                        $"{{{DataFilter.FieldJsonPropertyName} :'Firstname', {DataFilter.OperatorJsonPropertyName} :'isnotnull'}}",
                        typeof(DataFilter),
                        ((Expression<Func<object, bool>>) ((result) => result is DataFilter
                            && "Firstname" == ((DataFilter)result).Field
                            && Operators["isnotnull"] == ((DataFilter)result).Operator
                            && ((DataFilter)result).Value == null))
                    };



            }
        }

        /// <summary>
        /// Deserialize tests cases
        /// </summary>
        public static IEnumerable<object[]> SerializeCases
        {
            get
            {
                
                foreach (KeyValuePair<string, DataFilterOperator> item in Operators.Where(kv => !DataFilter.UnaryOperators.Any(op => op  == kv.Value)))
                {
                    yield return new object[]{
                        new DataFilter(field : "Firstname", @operator : item.Value, value : "Bruce"),
                        ((Expression<Func<string, bool>>) ((json) => json != null
                            && JToken.Parse(json).Type == JTokenType.Object
                            && JObject.Parse(json).Properties().Count() == 3
                            && "Firstname".Equals(JObject.Parse(json)[DataFilter.FieldJsonPropertyName].Value<string>())
                            && item.Key.Equals(JObject.Parse(json)[DataFilter.OperatorJsonPropertyName].Value<string>())
                            && "Bruce".Equals(JObject.Parse(json)[DataFilter.ValueJsonPropertyName].Value<string>())))


                    };
                }

                foreach (KeyValuePair<string, DataFilterOperator> item in Operators.Where(kv => DataFilter.UnaryOperators.Any(op => op == kv.Value)))
                {
                    yield return new object[]{
                        new DataFilter(field : "Firstname", @operator : item.Value),
                        ((Expression<Func<string, bool>>) ((json) => json != null
                            && JToken.Parse(json).Type == JTokenType.Object
                            && JObject.Parse(json).Properties().Count() == 2
                            && "Firstname".Equals(JObject.Parse(json)[DataFilter.FieldJsonPropertyName].Value<string>())
                            && item.Key.Equals(JObject.Parse(json)[DataFilter.OperatorJsonPropertyName].Value<string>())))

                    };
                }

            }
        }


        /// <summary>
        /// Tests the deserialization of the <paramref name="json"/> to an instance of the specified <paramref name="targetType"/> <br/>
        /// The deserialization is done using <c>JsonConvert.DeserializeObject</c>
        /// </summary>
        /// <param name="json">json to deserialize</param>
        /// <param name="targetType">type the json string will be deserialize into</param>
        /// <param name="expectation">Expectation that result of the deserialization should match</param>
        [Theory]
        [MemberData(nameof(DeserializeCases))]
        public void Deserialize(string json, Type targetType, Expression<Func<object, bool>> expectation)
        {
            _outputHelper.WriteLine($"{nameof(json)} : {json}");

            object result = JsonConvert.DeserializeObject(json, targetType);

            result.Should()
                .Match(expectation);
        }


        /// <summary>
        /// Tests the serialization of the <paramref name="obj"/> to its string representation
        /// The deserialization is done using <c>JsonConvert.DeserializeObject</c>
        /// </summary>
        /// <param name="filter">json to deserialize</param>
        /// <param name="expectation">Expectation that result of the deserialization should match</param>
        [Theory]
        [MemberData(nameof(SerializeCases))]
        public void Serialize(IDataFilter filter, Expression<Func<string, bool>> expectation)
        {
            _outputHelper.WriteLine($"Serializing {filter}");

            string result = JsonConvert.SerializeObject(filter);

            result.Should()
                .Match(expectation);
        }


        


        public void Dispose()
        {
            _outputHelper = null;
        }


    }
}

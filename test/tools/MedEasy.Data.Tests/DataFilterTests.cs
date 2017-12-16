using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.Data.DataFilterOperator;
using FluentAssertions;
using static MedEasy.Data.DataFilterLogic;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Data.Tests
{
    public class DataFilterTests
    {

        private readonly ITestOutputHelper _output;
        private static IImmutableDictionary<string, DataFilterOperator> _operators = new Dictionary<string, DataFilterOperator>
        {
            ["contains"] = Contains,
            ["endswith"] = EndsWith,
            ["eq"] = EqualTo,
            ["gt"] = GreaterThan,
            ["gte"] = GreaterThanOrEqual,
            ["isempty"] = IsEmpty,
            ["isnotempty"] = IsNotEmpty,
            ["isnotnull"] = IsNotNull,
            ["isnull"] = IsNull,
            ["lt"] = LessThan,
            ["lte"] = LessThanOrEqualTo,
            ["neq"] = NotEqualTo,
            ["startswith"] = StartsWith
        }.ToImmutableDictionary();


        /// <summary>
        /// Serialization of instance of <see cref="DataFilter"/> test cases
        /// </summary>
        public static IEnumerable<object[]> DataFilterToJsonCases
        {
            get
            {
                yield return new object[]
                {
                    new DataFilter (field : "Firstname", @operator  : EqualTo,  value : "Batman"),
                    ((Expression<Func<string, bool>>)(json =>
                        "Firstname".Equals((string) JObject.Parse(json)[DataFilter.FieldJsonPropertyName]) &&
                        "eq".Equals((string) JObject.Parse(json)[DataFilter.OperatorJsonPropertyName]) &&
                        "Batman".Equals((string) JObject.Parse(json)[DataFilter.ValueJsonPropertyName])
                    ))
                };
            }
        }

        /// <summary>
        /// Deserialization of various json representation into <see cref="DataFilter"/>
        /// </summary>
        public static IEnumerable<object[]> DataFilterDeserializeCases
        {
            get
            {
                foreach (KeyValuePair<string, DataFilterOperator> item in _operators)
                {
                    yield return new object[]
                    {

                        $"{{ field = 'Firstname', operator = '{item.Key}',  Value = 'Batman'}}",
                        ((Expression<Func<IDataFilter, bool>>)(result => result is DataFilter
                            && "Firstname".Equals(((DataFilter) result).Field)
                            && item.Value.Equals(((DataFilter) result).Operator) &&
                            "Batman".Equals(((DataFilter) result).Value)
                        ))
                    };
                }
            }
        }

        public static IEnumerable<object[]> CollectionOfDataFiltersCases
        {
            get
            {
                yield return new object[] {
                    new IDataFilter[]
                    {
                        new DataFilter (field : "Firstname", @operator : EqualTo, value : "Bruce"),
                        new DataFilter (field : "Lastname", @operator : EqualTo, value : "Wayne" )
                    },
                    ((Expression<Func<string, bool>>)(json =>
                        JToken.Parse(json).Type == JTokenType.Array
                        && JArray.Parse(json).Count == 2


                        && JArray.Parse(json)[0].Type == JTokenType.Object
                        && JArray.Parse(json)[1].IsValid(DataFilter.Schema(EqualTo))
                        && JArray.Parse(json)[0][DataFilter.FieldJsonPropertyName].Value<string>() == "Firstname"
                        && JArray.Parse(json)[0][DataFilter.OperatorJsonPropertyName].Value<string>() == "eq"
                        && JArray.Parse(json)[0][DataFilter.ValueJsonPropertyName].Value<string>() == "Bruce"

                        && JArray.Parse(json)[1].Type == JTokenType.Object
                        && JArray.Parse(json)[1].IsValid(DataFilter.Schema(EqualTo))
                        && JArray.Parse(json)[1][DataFilter.FieldJsonPropertyName].Value<string>() == "Firstname"
                        && JArray.Parse(json)[1][DataFilter.OperatorJsonPropertyName].Value<string>() == "eq"
                        && JArray.Parse(json)[1][DataFilter.ValueJsonPropertyName].Value<string>() == "Wayne"
                    ))

                };
            }
        }

        public static IEnumerable<object[]> DataCompositeFilterToJsonCases
        {
            get
            {
                yield return new object[]
                {
                    new DataCompositeFilter  {
                        Logic = Or,
                        Filters = new [] {
                            new DataFilter (field : "Nickname", @operator : EqualTo, value : "Batman"),
                            new DataFilter (field : "Nickname", @operator : EqualTo, value : "Robin")
                        }
                    },
                    ((Expression<Func<string, bool>>)(json =>
                        JObject.Parse(json).Properties().Count() == 2 &&

                        "or".Equals((string) JObject.Parse(json)[DataCompositeFilter.LogicJsonPropertyName]) &&

                        "Nickname".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][0][DataFilter.FieldJsonPropertyName]) &&
                        "eq".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][0][DataFilter.OperatorJsonPropertyName]) &&
                        "Batman".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][0][DataFilter.ValueJsonPropertyName])
                               &&
                        "Nickname".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][1][DataFilter.FieldJsonPropertyName]) &&
                        "eq".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][1][DataFilter.OperatorJsonPropertyName]) &&
                        "Robin".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][1][DataFilter.ValueJsonPropertyName])

                    ))
                };

                yield return new object[]
                {
                    new DataCompositeFilter  {
                        Filters = new [] {
                            new DataFilter (field : "Nickname", @operator : EqualTo, value : "Batman"),
                            new DataFilter (field : "Nickname", @operator : EqualTo, value : "Robin")

                        }
                    },
                    ((Expression<Func<string, bool>>)(json =>
                        JObject.Parse(json).Properties().Count() == 2 &&

                        "and".Equals((string) JObject.Parse(json)[DataCompositeFilter.LogicJsonPropertyName]) &&

                        "Nickname".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][0][DataFilter.FieldJsonPropertyName]) &&
                        "eq".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][0][DataFilter.OperatorJsonPropertyName]) &&
                        "Batman".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][0][DataFilter.ValueJsonPropertyName])
                               &&
                        "Nickname".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][1][DataFilter.FieldJsonPropertyName]) &&
                        "eq".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][1][DataFilter.OperatorJsonPropertyName]) &&
                        "Robin".Equals((string)JObject.Parse(json)[DataCompositeFilter.FiltersJsonPropertyName][1][DataFilter.ValueJsonPropertyName])

                    ))
                };

            }
        }


        public static IEnumerable<object[]> DataFilterSchemaTestCases
        {
            get
            {
                yield return new object[]
                {
                    $"{{{DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq', {DataFilter.ValueJsonPropertyName} :'batman' }}",
                    EqualTo,
                    true
                };

                yield return new object[]
                {
                    $"{{{DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq', {DataFilter.ValueJsonPropertyName} : null }}",
                    EqualTo,
                    false
                };

                yield return new object[]
                {
                    $"{{{DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq' }}",
                    EqualTo,
                    false
                };

                yield return new object[]
                {
                    $"{{{DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'contains', {DataFilter.ValueJsonPropertyName} : 'br' }}",
                    Contains,
                    true
                };

                yield return new object[]
                {
                    $"{{{DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'contains', {DataFilter.ValueJsonPropertyName} : 6 }}",
                    Contains,
                    false
                };

                yield return new object[]
                {
                    $"{{{DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'isnull', {DataFilter.ValueJsonPropertyName} : 6 }}",
                    IsNull,
                    false
                };
            }
        }


        public static IEnumerable<object[]> DataCompositeFilterSchemaTestCases
        {
            get
            {
                yield return new object[]
                {
                    "{" +
                        $"{DataCompositeFilter.LogicJsonPropertyName} : 'or'," +
                        $"{DataCompositeFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq', {DataFilter.ValueJsonPropertyName} : 'batman' }}," +
                            $"{{ {DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq', {DataFilter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    true
                };

                yield return new object[]
                {
                    "{" +
                        $"{DataCompositeFilter.LogicJsonPropertyName} : 'and'," +
                        $"{DataCompositeFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq', {DataFilter.ValueJsonPropertyName} : 'batman' }}," +
                            $"{{ {DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq', {DataFilter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    true
                };

                yield return new object[]
                {
                    "{" +
                        $"{DataCompositeFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq', {DataFilter.ValueJsonPropertyName} : 'batman' }}," +
                            $"{{ {DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq', {DataFilter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    true
                };

                yield return new object[]
                {
                    "{" +
                        $"{DataCompositeFilter.FiltersJsonPropertyName}: [" +
                            $"{{ {DataFilter.FieldJsonPropertyName} : 'nickname', {DataFilter.OperatorJsonPropertyName} : 'eq', {DataFilter.ValueJsonPropertyName} : 'robin' }}" +
                        "]" +
                    "}",
                    false
                };

            }
        }



        public DataFilterTests(ITestOutputHelper output)
        {
            _output = output;
        }


        [Theory]
        [MemberData(nameof(DataFilterToJsonCases))]
        public void DataFilterToJson(DataFilter filter, Expression<Func<string, bool>> jsonMatcher)
            => ToJson(filter, jsonMatcher);


        [Theory]
        [MemberData(nameof(DataCompositeFilterToJsonCases))]
        public void DataCompositeFilterToJson(DataCompositeFilter filter, Expression<Func<string, bool>> jsonMatcher)
            => ToJson(filter, jsonMatcher);


        [Theory]
        [MemberData(nameof(CollectionOfDataFiltersCases))]
        public void CollectionOfFiltersToJson(IEnumerable<IDataFilter> filters, Expression<Func<string, bool>> jsonExpectation)
        {
            string json = SerializeObject(filters);

            json.Should().Match(jsonExpectation);
        }

        private void ToJson(IDataFilter filter, Expression<Func<string, bool>> jsonMatcher)
        {
            _output.WriteLine($"Testing : {filter}{Environment.NewLine} against {Environment.NewLine} {jsonMatcher} ");
            filter.ToJson().Should().Match(jsonMatcher);
        }


        [Theory]
        [MemberData(nameof(DataFilterSchemaTestCases))]
        public void DataFilterSchema(string json, DataFilterOperator @operator, bool expectedValidity)
        {
            _output.WriteLine($"{nameof(json)} : {json}");
            _output.WriteLine($"{nameof(DataFilterOperator)} : {@operator}");


            // Arrange
            JSchema schema = DataFilter.Schema(@operator);

            // Act
            bool isValid = JObject.Parse(json).IsValid(schema);

            // Arrange
            isValid.Should().Be(expectedValidity);
        }

        public static IEnumerable<object[]> DataFilterEquatableCases
        {
            get
            {
                yield return new object[]
                {
                    new DataFilter("property", EqualTo, "value"),
                    new DataFilter("property", EqualTo, "value"),
                    true
                };

                yield return new object[]
                {
                    new DataFilter("property", EqualTo, null),
                    new DataFilter("property", IsNull),
                    true
                };

                yield return new object[]
                {
                    new DataFilter("property", EqualTo, "value"),
                    new DataFilter("property", NotEqualTo, "value"),
                    false
                };

                yield return new object[]
                {
                    new DataFilter("Property", EqualTo, "value"),
                    new DataFilter("property", EqualTo, "value"),
                    false
                };



                {
                    DataFilter first = new DataFilter("Property", EqualTo, "value");
                    yield return new object[]
                    {
                        first,
                        first,
                        true
                    };
                }

            }
        }

        [Theory]
        [MemberData(nameof(DataFilterEquatableCases))]
        public void DataFilterImplementsEquatableProperly(DataFilter first, DataFilter second, bool expectedResult)
        {
            _output.WriteLine($"first : {first}");
            _output.WriteLine($"second : {second}");

            // Act
            bool result = first.Equals(second);

            // Assert
            result.Should().Be(expectedResult);
        }


        [Theory]
        [MemberData(nameof(DataCompositeFilterSchemaTestCases))]
        public void DataCompositeFilterSchema(string json, bool expectedValidity)
        {
            _output.WriteLine($"{nameof(json)} : {json}");

            // Arrange
            JSchema schema = DataCompositeFilter.Schema;

            // Act
            bool isValid = JObject.Parse(json).IsValid(schema);

            // Assert
            isValid.Should().Be(expectedValidity);
        }
    }
}

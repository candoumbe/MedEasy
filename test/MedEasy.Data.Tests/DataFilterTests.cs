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
        private static IImmutableDictionary<string, DataFilterOperator> Operators = new Dictionary<string, DataFilterOperator>
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
                    new DataFilter { Field = "Firstname", Operator = EqualTo,  Value = "Batman"},
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
                foreach (var item in Operators)
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
                        new DataFilter { Field = "Firstname", Operator = EqualTo, Value = "Bruce" },
                        new DataFilter { Field = "Lastname", Operator = EqualTo, Value = "Wayne" }
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
                            new DataFilter { Field = "Nickname", Operator = EqualTo,  Value = "Batman" },
                            new DataFilter { Field = "Nickname", Operator = EqualTo,  Value = "Robin" },

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
                            new DataFilter { Field = "Nickname", Operator = EqualTo,  Value = "Batman" },
                            new DataFilter { Field = "Nickname", Operator = EqualTo,  Value = "Robin" },

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


        public static IEnumerable<object> DataFilterSchemaTestCases
        {
            get
            {
                yield return new object[]
                {
                    new DataFilter { Field = "Firstname", Operator = EqualTo, Value = "Bruce" },
                    true
                };

                yield return new object[]
                {
                    new DataFilter { Field = "Firstname", Operator = EqualTo, Value = null },
                    false
                };

                yield return new object[]
                {
                    new DataFilter { Field = "Firstname", Operator = EqualTo },
                    false
                };

                yield return new object[]
                {
                    new DataFilter { Field = "Firstname", Operator = Contains, Value = "Br"},
                    true
                };

                yield return new object[]
                {
                    new DataFilter { Field = "Firstname", Operator = Contains, Value = 6},
                    false
                };
            }
        }


        public static IEnumerable<object> DataCompositeFilterSchemaTestCases
        {
            get
            {
                yield return new object[]
                {
                    new DataCompositeFilter  {
                        Logic = Or,
                        Filters = new [] {
                            new DataFilter { Field = "Nickname", Operator = EqualTo,  Value = "Batman" },
                            new DataFilter { Field = "Nickname", Operator = EqualTo,  Value = "Robin" },
                        }
                    },
                    true
                };

                yield return new object[]
                {
                    new DataCompositeFilter  {
                        Filters = new [] {
                            new DataFilter { Field = "Firstname", Operator = EqualTo,  Value = "Bruce" },
                            new DataFilter { Field = "Lastname", Operator = EqualTo,  Value = "Wayne" },
                        }
                    },
                    true
                };

                yield return new object[]
                {
                    new DataCompositeFilter  {
                        Logic = Or,
                        Filters = new [] {
                            new DataFilter { Field = "Nickname", Operator = EqualTo,  Value = "Robin" },
                        }
                    },
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
        }

        private void ToJson(IDataFilter filter, Expression<Func<string, bool>> jsonMatcher)
        {
            _output.WriteLine($"Testing : {filter}{Environment.NewLine} against {Environment.NewLine} {jsonMatcher} ");
            filter.ToJson().Should().Match(jsonMatcher);
        }


        [Theory]
        [MemberData(nameof(DataFilterSchemaTestCases))]
        public void DataFilterSchema(DataFilter filter, bool expectedValidity)
            => Schema(filter, expectedValidity);


        public static IEnumerable<object> DataFilterEquatableCases
        {
            get
            {
                yield return new object[]
                {
                    new DataFilter(),
                    new DataFilter(),
                    true
                };

                yield return new object[]
                {
                    new DataFilter { Field = "firstname", Operator = EqualTo, Value = "bruce"},
                    new DataFilter { Field = "firstname", Operator = EqualTo, Value = "bruce"},
                    true
                };

                yield return new object[]
                {
                    new DataFilter { Field = "firstname", Operator = EqualTo, Value = "bruce"},
                    new DataFilter { Field = "firstname", Operator = NotEqualTo, Value = "bruce"},
                    false
                };

                yield return new object[]
                {
                    new DataFilter { Field = "firstname", Operator = EqualTo, Value = "bruce"},
                    new DataFilter { Field = "Firstname", Operator = EqualTo, Value = "bruce"},
                    false
                };

                {
                    DataFilter first = new DataFilter { Field = "prop", Operator = NotEqualTo, Value = true };
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
        public void DataCompositeFilterSchema(DataCompositeFilter filter, bool expectedValidity)
            => Schema(filter, expectedValidity);


        private void Schema(IDataFilter filter, bool expectedValidity)
        {
            JSchema schema = filter is DataFilter
                ? DataFilter.Schema((filter as DataFilter).Operator)
                : DataCompositeFilter.Schema;

            _output.WriteLine($"Testing :{filter} {Environment.NewLine} against {Environment.NewLine} {schema}");

            JObject.Parse(filter.ToJson())
                  .IsValid(schema)
                  .Should().Be(expectedValidity);
        }



    }
}

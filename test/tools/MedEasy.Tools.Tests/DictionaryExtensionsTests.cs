using System.Collections.Generic;
using System;
using Xunit.Abstractions;
using System.Linq.Expressions;
using FluentAssertions;
using static System.StringSplitOptions;
using Xunit;
using static Newtonsoft.Json.JsonConvert;
using static MedEasy.Data.DataFilterOperator;
using FluentAssertions.Extensions;

namespace MedEasy.Tools.Tests
{
    /// <summary>
    /// unit tests for <see cref="DictionaryExtensions"/> methods.
    /// </summary>
    public class DictionaryExtensionsTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public DictionaryExtensionsTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }


        public void Dispose()
        {
            _outputHelper = null;
        }

        public static IEnumerable<object[]> ToQueryStringCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    ((Expression<Func<string, bool>>)(x => x == string.Empty))
                };

                
                yield return new object[]
                {
                    new Dictionary<string, object>{},
                    ((Expression<Func<string, bool>>)(x => x == string.Empty)),
                };
                yield return new object[]
                {
                    new Dictionary<string, object>{
                        ["limit"] = 1
                    },
                    ((Expression<Func<string, bool>>)(x => "limit=1".Equals(x)))
                };

                yield return new object[]
                {
                    new Dictionary<string, object>{
                        ["limit"] = null
                    },
                    ((Expression<Func<string, bool>>)(x => x == string.Empty))
                };

                yield return new object[]
               {
                    new Dictionary<string, object>{
                        ["date"] = 1.February(2010).AddMinutes(30)
                    },
                    ((Expression<Func<string, bool>>)(x => "date=2010-02-01T00:30:00".Equals(x)))
               };

                yield return new object[]
               {
                    new Dictionary<string, object>{
                        ["date"] = 1.February(2010)
                    },
                    ((Expression<Func<string, bool>>)(x => "date=2010-02-01T00:00:00".Equals(x)))
               };

                yield return new object[]
                 {
                    new Dictionary<string, object>{
                        ["limit"] = 1,
                        ["offset"] = 3
                    },
                    ((Expression<Func<string, bool>>)(x => "limit=1&offset=3".Equals(x)))
                 };



                yield return new object[]
                 {
                    new Dictionary<string, object>{
                        ["search"] = new Dictionary<string, object>
                        {
                            ["page"] = 1,
                            ["pageSize"] = 3,
                            ["filter"] = new Dictionary<string, object>
                            {
                                ["field"] = "firstname",
                                ["op"] = "eq",
                                ["value"] = "Bruce"
                            }
                        },
                    },
                    ((Expression<Func<string, bool>>)( queryString =>
                        queryString != null &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Length == 5 &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[page]")}=1") &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[pageSize]")}=3") &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][field]")}=firstname") &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][op]")}=eq") &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][value]")}=Bruce")

                    ))
                 };

                yield return new object[]
                 {
                    new Dictionary<string, object>{
                        ["search"] = new Dictionary<string, object>
                        {
                            ["page"] = 1,
                            ["pageSize"] = 3,
                            ["filter"] = new Dictionary<string, object>
                            {
                                ["field"] = "firstname",
                                ["op"] = EqualTo,
                                ["value"] = "Bruce"
                            }
                        },
                    },
                    ((Expression<Func<string, bool>>)( queryString =>
                        queryString != null &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Length == 5 &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[page]")}=1") &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[pageSize]")}=3") &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][field]")}=firstname") &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][op]")}=EqualTo") &&
                        queryString.Split(new []{ "&"}, RemoveEmptyEntries).Once(x => x == $"{Uri.EscapeDataString("search[filter][value]")}=Bruce")

                    ))
                 };
            }
        }


        /// <summary>
        /// Tests <see cref="System.Collections.Generic.DictionaryExtensions.ToQueryString(IDictionary{string, object})"/>
        /// </summary>
        /// <param name="input">dictionary to turn into query</param>
        /// <param name="expectedString"></param>
        [Theory]
        [MemberData(nameof(ToQueryStringCases))]
        public void ToQueryString(IDictionary<string, object> input, Expression<Func<string, bool>> expectedString)
        {
            _outputHelper.WriteLine($"input : {SerializeObject(input)}");

            // Act
            string queryString = input?.ToQueryString(); 
            
            // Arrange
            queryString?.Should().Match(expectedString);
        }



    }
}

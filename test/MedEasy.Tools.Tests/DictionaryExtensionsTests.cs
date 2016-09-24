using System.Collections.Generic;
using Xunit;
using System;
using FluentAssertions;

namespace MedEasy.Tools.Tests
{

    public class DictionaryExtensionsTests
    {

        public static IEnumerable<object> ToQueryStringCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    string.Empty
                };

                yield return new object[]
                {
                    new Dictionary<string, object>{},
                    string.Empty,
                };
                yield return new object[]
                {
                    new Dictionary<string, object>{
                        ["limit"] = 1
                    },
                    "limit=1"
                };

                yield return new object[]
                 {
                    new Dictionary<string, object>{
                        ["limit"] = 1,
                        ["offset"] = 3
                    },
                    "limit=1&offset=3"
                 };
            }
        }


        [Theory]
        [MemberData(nameof(ToQueryStringCases))]
        public void ToQueryString(IDictionary<string, object> input, string expectedString)
            => input?.ToQueryString()?.Should().Be(expectedString);

    } 
}

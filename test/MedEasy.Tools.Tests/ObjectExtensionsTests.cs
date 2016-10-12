using System.Collections.Generic;
using Xunit;
using System;
using FluentAssertions;

namespace MedEasy.Tools.Tests
{
    /// <summary>
    /// Extensions methods for <see cref="Object"/> type.
    /// </summary>
    public class ObjectExtensionsTests
    {
        public static IEnumerable<object[]> ToQueryStringCases
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
                    new {},
                    string.Empty,
                };
                yield return new object[]
                {
                    new { limit = 1 },
                    "limit=1"
                };

                yield return new object[]
                 {
                    new { limit = 1, offset=3 },
                    "limit=1&offset=3"
                 };

                yield return new object[]
                {
                    new {limit = new [] {0, 1, 2, 3}},
                    "limit[0]=0&limit[1]=1&limit[2]=2&limit[3]=3"
                };
            }
        }

        [Theory]
        [MemberData(nameof(ToQueryStringCases))]
        public void ToQueryString(object input, string expectedString)
            => input?.ToQueryString()?.Should().Be(expectedString);

    } 
}

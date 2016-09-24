using System.Collections.Generic;
using Xunit;
using System;
using FluentAssertions;

namespace MedEasy.Tools.Tests
{

    public class StringExtensionsTests
    {

        public static IEnumerable<object> ToTitleCaseCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    null
                };

                yield return new object[]
                {
                    "bruce",
                    "Bruce"
                };
                yield return new object[]
                {
                    " bruce",
                    " Bruce"
                };

                yield return new object[]
                {
                    "bruce wayne",
                    "Bruce Wayne"
                };
            }
        }


        [Theory]
        [MemberData(nameof(ToTitleCaseCases))]
        public void ToTitleCase(string input, string expectedString)
            => input?.ToTitleCase()?.Should().Be(expectedString);

    } 
}

using FluentAssertions;
using MedEasy.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Core.UnitTests.Attributes
{
    [UnitTest]
    [Feature("Validation")]
    public class RequireNonDefaultAttributeTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private RequireNonDefaultAttribute _sut;


        public class Person
        {
            public string Name { get; set; }

        }

        [RequireNonDefault]
        public class Minion : Person
        {
            
            public int? EyesCount { get; set; }
        }

        public RequireNonDefaultAttributeTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _sut = new RequireNonDefaultAttribute();
        }

        public void Dispose() => _sut = null;


        [Fact]
        public void IsProperlySet() =>
            // Assert
            typeof(RequireNonDefaultAttribute).Should()
                .BeAssignableTo<ValidationAttribute>().And
                .BeDecoratedWith<AttributeUsageAttribute>(attr =>
                    !attr.AllowMultiple
                    && attr.ValidOn == (AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Class)
                    && !attr.Inherited);


        public static IEnumerable<object[]> ValidateCases
        {
            get
            {
                yield return new object[] { default(Guid), false, "default(Guid) must not be valid" };
                yield return new object[] { Guid.Empty, false, "Guid.Empty must not be valid" };
                yield return new object[] { Guid.Parse("79c92f74-42db-40f1-b9ae-685e00b2c93b"), true, "Guid.Parse('79c92f74-42db-40f1-b9ae-685e00b2c93b') must not be valid" };
                yield return new object[] { default(int?), false, "default(int?) must not be valid" };
                yield return new object[] { default(long?), false, "default(long?) must not be valid" };
                yield return new object[] { default(float?), false, "default(float?) must not be valid" };
                yield return new object[] { default(decimal?), false, "default(decimal?) must not be valid" };
                yield return new object[] { default(decimal), false, "default(decimal) must not be valid" };
                yield return new object[] { default(Minion), false, $"default({nameof(Minion)}) must not be valid" };
                yield return new object[] { new Minion(), false, $"instance of <{nameof(Minion)}> created with default constructor must not be valid" };
                yield return new object[] { new Minion { EyesCount = 2 }, true, $"instance of <{nameof(Minion)}> with one property set must be valid" };
                yield return new object[] { new Minion { Name = "Kevin" }, true, $"instance of <{nameof(Minion)}> with one property set must be valid" };

            }
        }

        [Theory]
        [InlineData(default(string), false, "default string is not valid")]
        [InlineData(default(int), false, "default int is not valid")]
        [InlineData(default(long), false, "default long is not valid")]
        [InlineData(default(float), false, "default float is not valid")]
        [InlineData(default(short), false, "default short is not valid")]
        [MemberData(nameof(ValidateCases))]
        public void Validate(object value, bool expectedResult, string reason)
        {
            _outputHelper.WriteLine($"Parameters : {SerializeObject(new { value, expectedResult })}");

            // Assert
            _sut.IsValid(value).Should()
                .Be(expectedResult, reason);
        }
    }
}

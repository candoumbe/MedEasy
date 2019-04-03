using FluentAssertions;
using MedEasy.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace MedEasy.AttributesUnitTests
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
                yield return new object[] { typeof(Guid), default(Guid), false, "default(Guid) must not be valid" };
                yield return new object[] { typeof(Guid), Guid.Empty, false, "Guid.Empty must not be valid" };
                yield return new object[] { typeof(Guid), Guid.Parse("79c92f74-42db-40f1-b9ae-685e00b2c93b"), true, "Guid.Parse('79c92f74-42db-40f1-b9ae-685e00b2c93b') must not be valid" };
                yield return new object[] { typeof(int?), default(int?), false, "default(int?) must not be valid" };
                yield return new object[] { typeof(long?), default(long?), false, "default(long?) must not be valid" };
                yield return new object[] { typeof(float?), default(float?), false, "default(float?) must not be valid" };
                yield return new object[] { typeof(decimal?), default(decimal?), false, "default(decimal?) must not be valid" };
                yield return new object[] { typeof(decimal), default(decimal), false, "default(decimal) must not be valid" };
                yield return new object[] { typeof(Minion), default(Minion), false, $"default({nameof(Minion)}) must not be valid" };
                yield return new object[] { typeof(Minion), new Minion(), false, $"instance of <{nameof(Minion)}> created with default constructor must not be valid" };
                yield return new object[] { typeof(Minion), new Minion { EyesCount = 2 }, true, $"instance of <{nameof(Minion)}> with one property set must be valid" };
                yield return new object[] { typeof(Minion), new Minion { Name = "Kevin" }, true, $"instance of <{nameof(Minion)}> with one property set must be valid" };
            }
        }

        [Theory]
        [InlineData(typeof(string), default(string), false, "default string is not valid")]
        [InlineData(typeof(int), default(int), false, "default int is not valid")]
        [InlineData(typeof(long), default(long), false, "default long is not valid")]
        [InlineData(typeof(float), default(float), false, "default float is not valid")]
        [InlineData(typeof(short), default(short), false, "default short is not valid")]
        [MemberData(nameof(ValidateCases))]
        public void Validate(Type type, object value, bool expectedResult, string reason)
        {
            _outputHelper.WriteLine($"Parameters : {new {type, value, expectedResult }.Stringify()}");

            // Assert
            _sut.IsValid(value).Should()
                .Be(expectedResult, reason);
        }
    }
}

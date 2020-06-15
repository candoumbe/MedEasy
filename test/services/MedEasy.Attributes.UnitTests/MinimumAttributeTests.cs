using FluentAssertions;
using MedEasy.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace MedEasy.Core.UnitTests.Attributes
{
    [UnitTest]
    [Feature("Validation")]
    public class MinimumAttributeTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private MinimumAttribute _sut;

        public MinimumAttributeTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
            _outputHelper = null;
            _sut = null;
        }

        [Fact]
        public void IsProperlySet()
        {
            // Arrange
            Type minimunAttributeType = typeof(MinimumAttribute);

            // Assert
            minimunAttributeType.Should()
                .BeAssignableTo<RangeAttribute>();

            IEnumerable<CustomAttributeData> attributes = minimunAttributeType.GetCustomAttributesData();

            attributes.Should()
                .HaveCount(1).And
                .ContainSingle(attr => attr.AttributeType == typeof(AttributeUsageAttribute),$"{nameof(AttributeUsageAttribute)} must be present on the class");

            AttributeUsageAttribute usage = minimunAttributeType.GetCustomAttribute<AttributeUsageAttribute>();
            usage.AllowMultiple.Should().BeFalse("attribute cannot be used multiple times on the same element");
            usage.ValidOn.Should().Be(AttributeTargets.Parameter | AttributeTargets.Property, "attribute can target both parameters and properties");
            usage.Inherited.Should().BeFalse("attribute cannot be herited");
        }
    }
}

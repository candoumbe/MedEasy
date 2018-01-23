using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.RestObjects.Tests
{
    public class FormFieldAttributeTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public FormFieldAttributeTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void Ctor_Should_Build_Valid_Instance()
        {

            // Act
            FormFieldAttribute attribute = new FormFieldAttribute();

            // Assert
            attribute.Type.Should().Be(FormFieldType.String);
            attribute.Enabled.Should()
                .BeNull();
            attribute.Mandatory.Should()
                .BeNull();
            attribute.Pattern.Should()
                .BeNull();
            attribute.Relations.Should()
                .BeAssignableTo<IEnumerable<string>>().And
                .BeEmpty();

            
        }

        [Fact]
        public void IsValid()
        {
            // Arrange
            TypeInfo typeInfo = typeof(FormFieldAttribute)
                .GetTypeInfo();

            // Act
            AttributeUsageAttribute attr = typeInfo.GetCustomAttribute<AttributeUsageAttribute>();

            // Assert
            attr.AllowMultiple.Should()
                .BeTrue($"multiple usage of {nameof(FormFieldAttribute)} on the same element is allowed");
            attr.Inherited.Should()
                .BeTrue("the attribute must propagate to inherited classes ");

            attr.ValidOn.Should()
                .Be(AttributeTargets.Property, $"{nameof(FormFieldAttribute)} can only be applied onto properties");

        }
    }
}

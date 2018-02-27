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
                .BeFalse();
            attribute.Required.Should()
                .BeFalse();
            attribute.Pattern.Should()
                .BeNull();
            attribute.Relations.Should()
                .BeAssignableTo<IEnumerable<string>>().And
                .BeEmpty();

            
        }

        [Fact]
        public void IsValid() => typeof(FormFieldAttribute).Should()
                .BeDecoratedWith<AttributeUsageAttribute>(attr =>
                    attr.AllowMultiple
                    && attr.Inherited
                    && attr.ValidOn == AttributeTargets.Property
                );
    }
}

namespace MedEasy.RestObjects.Tests
{
    using FluentAssertions;

    using System;

    using Xunit;
    using Xunit.Abstractions;

    public class FormFieldTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public FormFieldTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;


        [Fact]
        public void Ctor()
        {
            // Act
            FormField instance = new();

            // Assert
            instance.Description.Should().BeNull();
            instance.Enabled.Should().BeNull();
            instance.Label.Should().BeNull();
            instance.Max.Should().BeNull();
            instance.MaxLength.Should().BeNull();
            instance.MinLength.Should().BeNull();
            instance.Min.Should().BeNull();
            instance.Name.Should().BeNull();
            instance.Pattern.Should().BeNull();
            instance.Placeholder.Should().BeNull();
            instance.Required.Should().BeNull();
            instance.Secret.Should().BeNull();
            instance.Type.Should().Be(FormFieldType.String);
        }
    }
}

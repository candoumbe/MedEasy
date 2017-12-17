﻿using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.RestObjects.Tests
{
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
            FormField instance = new FormField();

            // Assert
            instance.Description.Should().BeNull();
            instance.Enabled.Should().BeNull();
            instance.Label.Should().BeNull();
            instance.MaxLength.Should().BeNull();
            instance.MinLength.Should().BeNull();
            instance.Name.Should().BeNull();
            instance.Pattern.Should().BeNull();
            instance.Placeholder.Should().BeNull();
            instance.Required.Should().BeNull();
            instance.Secret.Should().BeNull();
            instance.Type.Should().Be(FormFieldType.String);

        }
    }
}
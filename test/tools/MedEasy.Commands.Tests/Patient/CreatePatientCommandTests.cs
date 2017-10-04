using FluentAssertions;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Patient
{
    public class CreatePatientCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreatePatientCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new CreatePatientCommand(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void CtorShouldBuildUniqueInstanceOfCommand()
        {
            // Arrange
            CreatePatientInfo info = new CreatePatientInfo
            {
                Lastname = "Joker"
            };

            // Act
            CreatePatientCommand first = new CreatePatientCommand(info);
            CreatePatientCommand second = new CreatePatientCommand(info);

            // Assert
            first.Should().NotBeSameAs(second);
            first.Id.Should().NotBe(second.Id);
            first.Data.ShouldBeEquivalentTo(second.Data);
            first.Data.Should().NotBeSameAs(second.Data, $"two {nameof(CreatePatientCommand)} instances built from shared data should not share state.");
        }

        [Fact]
        public void CtorShouldBuildValidCommand()
        {

            // Arrange
            CreatePatientInfo info = new CreatePatientInfo
            {
                Lastname = "Joker"
            };

            // Act
            CreatePatientCommand command = new CreatePatientCommand(info);


            // Assert
            command.Id.Should().NotBeEmpty();
            command.Data.ShouldBeEquivalentTo(info);
            command.Data.Should().NotBeSameAs(info, $"Risk of shared state. Use {nameof(ObjectExtensions.DeepClone)}<T>() to keep a copy of the command's data");
        }
    }
}

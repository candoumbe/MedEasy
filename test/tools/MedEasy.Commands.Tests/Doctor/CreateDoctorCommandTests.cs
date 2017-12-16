using FluentAssertions;
using MedEasy.Commands.Doctor;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Doctor
{
    public class CreateDoctorCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreateDoctorCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new CreateDoctorCommand(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void CtorShouldBuildUniqueInstanceOfCommand()
        {
            // Arrange
            CreateDoctorInfo info = new CreateDoctorInfo
            {
                Lastname = "Joker"
            };

            // Act
            CreateDoctorCommand first = new CreateDoctorCommand(info);
            CreateDoctorCommand second = new CreateDoctorCommand(info);

            // Assert
            first.Should().NotBeSameAs(second);
            first.Id.Should().NotBe(second.Id);
            first.Data.ShouldBeEquivalentTo(second.Data);
            first.Data.Should().NotBeSameAs(second.Data, $"two {nameof(CreateDoctorCommand)} instances built from shared data should not share state.");
        }

        [Fact]
        public void CtorShouldBuildValidCommand()
        {

            // Arrange
            CreateDoctorInfo info = new CreateDoctorInfo
            {
                Lastname = "Joker"
            };

            // Act
            CreateDoctorCommand command = new CreateDoctorCommand(info);


            // Assert
            command.Id.Should().NotBeEmpty();
            command.Data.ShouldBeEquivalentTo(info);
            command.Data.Should().NotBeSameAs(info, $"Risk of shared state. Use {nameof(ObjectExtensions.DeepClone)}<T>() to keep a copy of the command's data");
        }
    }
}

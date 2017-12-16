using FluentAssertions;
using MedEasy.Commands.Specialty;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Specialty
{
    public class CreateSpecialtyCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreateSpecialtyCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new CreateSpecialtyCommand(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void CtorShouldBuildUniqueInstanceOfCommand()
        {
            // Arrange
            CreateSpecialtyInfo info = new CreateSpecialtyInfo
            {
                Name = "Médecine générale"
            };

            // Act
            CreateSpecialtyCommand first = new CreateSpecialtyCommand(info);
            CreateSpecialtyCommand second = new CreateSpecialtyCommand(info);

            // Assert
            first.Should().NotBeSameAs(second);
            first.Id.Should().NotBe(second.Id);
            first.Data.ShouldBeEquivalentTo(second.Data);
            first.Data.Should().NotBeSameAs(second.Data, $"two {nameof(CreateSpecialtyCommand)} instances built from shared data should not share state.");
        }

        [Fact]
        public void CtorShouldBuildValidCommand()
        {

            // Arrange
            CreateSpecialtyInfo info = new CreateSpecialtyInfo
            {
                Name = "Médecine générale"
            };

            // Act
            CreateSpecialtyCommand command = new CreateSpecialtyCommand(info);


            // Assert
            command.Id.Should().NotBeEmpty();
            command.Data.ShouldBeEquivalentTo(info);
            command.Data.Should().NotBeSameAs(info, $"Risk of shared state. Use {nameof(ObjectExtensions.DeepClone)}<T>() to keep a copy of the command's data");
        }
    }
}

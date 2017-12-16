using FluentAssertions;
using MedEasy.Commands.Patient;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Patient
{
    public class DeletePatientByIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public DeletePatientByIdCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new DeletePatientByIdCommand(Guid.Empty);

            // Assert
            action.ShouldThrow<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        
        [Fact]
        public void CtorShouldBuildValidCommand()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            // Act
            DeletePatientByIdCommand command = new DeletePatientByIdCommand(id);
            
            // Assert
            command.Id.Should().NotBeEmpty();
            command.Data.ShouldBeEquivalentTo(id);
        }
    }
}

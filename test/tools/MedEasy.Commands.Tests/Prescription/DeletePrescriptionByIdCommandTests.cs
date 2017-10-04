using FluentAssertions;
using MedEasy.Commands.Prescription;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Prescription
{
    public class DeletePrescriptionByIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public DeletePrescriptionByIdCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new DeletePrescriptionByIdCommand(Guid.Empty);

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
            DeletePrescriptionByIdCommand command = new DeletePrescriptionByIdCommand(id);
            
            // Assert
            command.Id.Should().NotBeEmpty();
            command.Data.ShouldBeEquivalentTo(id);
        }
    }
}

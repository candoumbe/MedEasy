using FluentAssertions;
using MedEasy.Commands.Appointment;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Appointment
{
    public class DeleteAppointmentByIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public DeleteAppointmentByIdCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new DeleteAppointmentByIdCommand(Guid.Empty);

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
            DeleteAppointmentByIdCommand command = new DeleteAppointmentByIdCommand(id);
            
            // Assert
            command.Id.Should().NotBeEmpty();
            command.Data.ShouldBeEquivalentTo(id);
        }
    }
}

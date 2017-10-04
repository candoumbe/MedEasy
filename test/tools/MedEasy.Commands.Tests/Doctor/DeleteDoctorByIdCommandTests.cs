﻿using FluentAssertions;
using MedEasy.Commands.Doctor;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Doctor
{
    public class DeleteDoctorByIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public DeleteDoctorByIdCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new DeleteDoctorByIdCommand(Guid.Empty);

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
            DeleteDoctorByIdCommand command = new DeleteDoctorByIdCommand(id);
            
            // Assert
            command.Id.Should().NotBeEmpty();
            command.Data.ShouldBeEquivalentTo(id);
        }
    }
}

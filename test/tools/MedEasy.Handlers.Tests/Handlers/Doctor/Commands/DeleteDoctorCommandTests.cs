using Moq;
using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using MedEasy.Commands.Doctor;
using Xunit;
using System.Collections.Generic;
using FluentAssertions;

namespace MedEasy.Handlers.Tests.Commands.Doctor
{
    public class DeleteDoctorCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;


        public DeleteDoctorCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

        }

        public static IEnumerable<object[]> CtorInvalidCases {
            get
            {
                yield return new object[] { null };
            }
        }

        /// <summary>
        /// Tests that a <see cref="DeleteDoctorByIdCommand"/> cannot be built with a <see cref="Guid.Empty"/>
        /// </summary>
        [Fact]
        public void CtorThrowsArgumentOutOfRangeException()
        {
            // Act
            Action action = () => new DeleteDoctorByIdCommand(Guid.Empty);

            // Assert
            action.ShouldThrow<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Tests that new <see cref="DeleteDoctorByIdCommand"/> instances have a non empty <see cref="DeleteDoctorByIdCommand.Id"/>.
        /// </summary>
        [Fact]
        public void IdOfCommandCannotBeEmpty()
        {
            // Act
            DeleteDoctorByIdCommand cmd = new DeleteDoctorByIdCommand(Guid.NewGuid());

            // Assert
            cmd.Id.Should()
                .NotBeEmpty($"{nameof(DeleteDoctorByIdCommand)}.{nameof(DeleteDoctorByIdCommand.Id)} must not be {Guid.Empty}");
        }

        
        
        public void Dispose()
        {
            _outputHelper = null;
        }
    }
}

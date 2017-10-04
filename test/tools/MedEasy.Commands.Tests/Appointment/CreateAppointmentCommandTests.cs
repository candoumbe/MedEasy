using FluentAssertions;
using MedEasy.Commands.Appointment;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Appointment
{
    public class CreateAppointmentCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreateAppointmentCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new CreateAppointmentCommand(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void CtorShouldBuildUniqueInstanceOfCommand()
        {
            // Arrange
            Guid doctorId = Guid.NewGuid();
            Guid patientId = Guid.NewGuid();

            CreateAppointmentInfo info = new CreateAppointmentInfo
            {
                PatientId = patientId,
                DoctorId = doctorId,
                Duration = 30,
                StartDate = 25.July(2009)
            };

            // Act
            CreateAppointmentCommand first = new CreateAppointmentCommand(info);
            CreateAppointmentCommand second = new CreateAppointmentCommand(info);

            // Assert
            first.Should().NotBeSameAs(second);
            first.Id.Should().NotBe(second.Id);
            first.Data.ShouldBeEquivalentTo(second.Data);
            first.Data.Should().NotBeSameAs(second.Data, $"two {nameof(CreateAppointmentCommand)} instances built from shared data should not share state.");
        }

        [Fact]
        public void CtorShouldBuildValidCommand()
        {

            // Arrange
            Guid doctorId = Guid.NewGuid();
            Guid patientId = Guid.NewGuid();

            CreateAppointmentInfo info = new CreateAppointmentInfo
            {
                PatientId = patientId,
                DoctorId = doctorId,
                Duration = 30,
                StartDate = 25.July(2009)
            };

            // Act
            CreateAppointmentCommand command = new CreateAppointmentCommand(info);


            // Assert
            command.Id.Should().NotBeEmpty();
            command.Data.ShouldBeEquivalentTo(info);
            command.Data.Should().NotBeSameAs(info, $"Risk of shared state. Use {nameof(ObjectExtensions.DeepClone)}<T>() to keep a copy of the command's data");
        }
    }
}

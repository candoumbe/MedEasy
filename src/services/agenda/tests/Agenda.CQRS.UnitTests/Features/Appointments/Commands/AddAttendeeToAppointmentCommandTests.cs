namespace Agenda.CQRS.UnitTests.Features.Appointments.Commmands
{
    using Agenda.CQRS.Features.Appointments.Commands;
    using Agenda.Ids;

    using FluentAssertions;

    using System;
    using System.Collections.Generic;

    using Xunit;
    using Xunit.Categories;

    [Feature("Agenda")]
    [UnitTest]
    public class AddAttendeeToAppointmentCommandTests
    {
        [Fact]
        public void Ctor_Is_Valid()
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();
            AttendeeId participantId = AttendeeId.New();

            // Act
            AddAttendeeToAppointmentCommand instance = new(data: (appointmentId, participantId));

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
            instance.Data.appointmentId.Should()
                .Be(appointmentId);
            instance.Data.attendeeId.Should()
                .Be(participantId);
        }

        [Fact]
        public void Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new AddAttendeeToAppointmentCommand(default);

            // Assert
            action.Should()
                .Throw<ArgumentException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                {
                    AppointmentId appointmentId = AppointmentId.New();
                    AttendeeId participantId = AttendeeId.New();

                    yield return new object[]
                    {
                        new AddAttendeeToAppointmentCommand((appointmentId, participantId)),
                        new AddAttendeeToAppointmentCommand((appointmentId, participantId)),
                        true,
                        $"two {nameof(AddAttendeeToAppointmentCommand)} commands with same data"
                    };
                }

                {
                    AppointmentId appointmentId = AppointmentId.New();
                    AttendeeId participantId = AttendeeId.New();

                    yield return new object[]
                    {
                        new AddAttendeeToAppointmentCommand((appointmentId, participantId)),
                        new AddAttendeeToAppointmentCommand((appointmentId, AttendeeId.New())),
                        false,
                        $"two {nameof(AddAttendeeToAppointmentCommand)} commands with different {nameof(AddAttendeeToAppointmentCommand.Data.attendeeId)} data"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void AreEquals(AddAttendeeToAppointmentCommand first, object second, bool expectedResult, string reason)
        {
            // Act
            bool actualResult = first.Equals(second);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }
    }
}

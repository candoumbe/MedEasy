using Agenda.CQRS.Features.Appointments.Commands;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Commmands
{
    [Feature("Agenda")]
    [UnitTest]
    public class AddParticipantToAppointmentCommandTests
    {
        [Fact]
        public void Ctor_Is_Valid()
        {
            // Arrange
            Guid appointmentId = Guid.NewGuid();
            Guid participantId = Guid.NewGuid();

            // Act
            AddParticipantToAppointmentCommand instance = new AddParticipantToAppointmentCommand(data : (appointmentId, participantId));

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
            instance.Data.appointmentId.Should()
                .Be(appointmentId);
            instance.Data.participantId.Should()
                .Be(participantId);
        }

        [Fact]
        public void Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new AddParticipantToAppointmentCommand(default);

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
                    Guid appointmentId = Guid.NewGuid();
                    Guid participantId = Guid.NewGuid();

                    yield return new object[]
                    {
                        new AddParticipantToAppointmentCommand((appointmentId, participantId)),
                        new AddParticipantToAppointmentCommand((appointmentId, participantId)),
                        true,
                        $"two {nameof(AddParticipantToAppointmentCommand)} commands with same data"
                    };
                }

                {
                    Guid appointmentId = Guid.NewGuid();
                    Guid participantId = Guid.NewGuid();

                    yield return new object[]
                    {
                        new AddParticipantToAppointmentCommand((appointmentId, participantId)),
                        new AddParticipantToAppointmentCommand((appointmentId, Guid.NewGuid())),
                        false,
                        $"two {nameof(AddParticipantToAppointmentCommand)} commands with different {nameof(AddParticipantToAppointmentCommand.Data.participantId)} data"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void AreEquals(AddParticipantToAppointmentCommand first, object second, bool expectedResult, string reason)
        {
            // Act
            bool actualResult = first.Equals(second);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }
    }
}

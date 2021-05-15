namespace Agenda.CQRS.UnitTests.Features.Appointments.Commmands
{
    using Agenda.CQRS.Features.Appointments.Commands;
    using Agenda.Ids;

    using FluentAssertions;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Xunit;
    using Xunit.Categories;

    [Feature("Agenda")]
    [UnitTest]
    public class RemoveAttendeeFromAppointmentByIdCommandTests
    {
        [Fact]
        public void ClassDefinition() => typeof(RemoveAttendeeFromAppointmentByIdCommand).Should()
                .BeDerivedFrom<CommandBase<Guid, (AppointmentId appointmentId, AttendeeId attendeeId), DeleteCommandResult>>().And
                .NotBeAbstract().And
                .NotHaveDefaultConstructor();

        [Fact]
        public void Ctor_Is_Valid()
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();
            AttendeeId participantId = AttendeeId.New();

            // Act
            RemoveAttendeeFromAppointmentByIdCommand instance = new(data: (appointmentId, participantId));

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
            instance.Data.appointmentId.Should()
                .Be(appointmentId);
            instance.Data.attendeeId.Should()
                .Be(participantId);
        }

        public static IEnumerable<object[]> InvalidParametersForCtorCases
        {
            get
            {
                AppointmentId[] appointmentIds = { AppointmentId.Empty, default, AppointmentId.New() };
                AttendeeId[] attendeeIds = { AttendeeId.Empty, default, AttendeeId.New() };

                return appointmentIds.CrossJoin(attendeeIds)
                                     .Where(tuple => tuple.Item1 == default || tuple.Item1 == null || tuple.Item2 == default || tuple.Item2 == null)
                                     .Select(tuple => new object[] { tuple.Item1, tuple.Item2 });
            }
        }

        [Theory]
        [MemberData(nameof(InvalidParametersForCtorCases))]
        public void Ctor_Throws_ArgumentException(AppointmentId appointmentId, AttendeeId participantId)
        {
            // Act
            Action action = () => new RemoveAttendeeFromAppointmentByIdCommand(data: (appointmentId, participantId));

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
                        new RemoveAttendeeFromAppointmentByIdCommand((appointmentId, participantId)),
                        new RemoveAttendeeFromAppointmentByIdCommand((appointmentId, participantId)),
                        true,
                        "two commands with same data"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void AreEquals(RemoveAttendeeFromAppointmentByIdCommand first, RemoveAttendeeFromAppointmentByIdCommand second, bool expectedResult, string reason)
        {
            // Act
            bool actualResult = first.Equals(second);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }
    }
}

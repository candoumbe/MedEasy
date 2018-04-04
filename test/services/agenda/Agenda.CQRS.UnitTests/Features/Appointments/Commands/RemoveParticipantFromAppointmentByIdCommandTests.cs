using Agenda.CQRS.Features.Appointments.Commands;
using FluentAssertions;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Commmands
{

    [Feature("Agenda")]
    [UnitTest]
    public class RemoveParticipantFromAppointmentByIdCommandTests
    {
        [Fact]
        public void ClassDefinition()
        {
            typeof(RemoveParticipantFromAppointmentByIdCommand).Should()
                .BeDerivedFrom<CommandBase<Guid, (Guid appointmentId, Guid participantId), DeleteCommandResult>>().And
                .NotBeAbstract().And
                .NotHaveDefaultConstructor();
        }


        [Fact]
        public void Ctor_Is_Valid()
        {
            // Arrange
            Guid appointmentId = Guid.NewGuid();
            Guid participantId = Guid.NewGuid();

            // Act
            RemoveParticipantFromAppointmentByIdCommand instance = new RemoveParticipantFromAppointmentByIdCommand(data : (appointmentId, participantId));

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
            instance.Data.appointmentId.Should()
                .Be(appointmentId);
            instance.Data.participantId.Should()
                .Be(participantId);
        }

        public static IEnumerable<object[]> InvalidParametersForCtorCases
        {
            get
            {
                Guid[] guids = { Guid.Empty, default, Guid.NewGuid() };

                return guids.CrossJoin(guids)
                    .Where(tuple => tuple.Item1 == default || tuple.Item2 == default)
                    .Select(tuple => new object[] { tuple.Item1, tuple.Item2});

            }
        }

        [Theory]
        [MemberData(nameof(InvalidParametersForCtorCases))]
        public void Ctor_Throws_ArgumentException(Guid appointmentId, Guid participantId)
        {
            // Act
            Action action = () => new RemoveParticipantFromAppointmentByIdCommand(data : (appointmentId, participantId));

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
                        new RemoveParticipantFromAppointmentByIdCommand((appointmentId, participantId)),
                        new RemoveParticipantFromAppointmentByIdCommand((appointmentId, participantId)),
                        true,
                        "two commands with same data"
                    };
                }
            }

        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void AreEquals(RemoveParticipantFromAppointmentByIdCommand first, RemoveParticipantFromAppointmentByIdCommand second, bool expectedResult, string reason)
        {
            // Act
            bool actualResult = first.Equals(second);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }
    }
}

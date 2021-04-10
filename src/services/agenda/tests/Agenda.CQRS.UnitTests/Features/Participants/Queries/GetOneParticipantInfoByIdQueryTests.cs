using Agenda.CQRS.Features.Participants.Queries;

using FluentAssertions;

using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Participants.Queries
{
    [Feature("Agenda")]
    [UnitTest]
    public class GetOneParticipantInfoByIdQueryTests
    {
        [Fact]
        public void Ctor_Is_Valid()
        {
            // Arrange
            Guid participantId = Guid.NewGuid();

            // Act
            GetOneAttendeeInfoByIdQuery instance = new(participantId);

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
            instance.Data.Should()
                .Be(participantId);
        }

        [Fact]
        public void Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new GetOneAttendeeInfoByIdQuery(default);

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
                        new GetOneAttendeeInfoByIdQuery(participantId),
                        new GetOneAttendeeInfoByIdQuery(participantId),
                        true,
                        $"two different {nameof(GetOneAttendeeInfoByIdQuery)} with same data"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void AreEquals(GetOneAttendeeInfoByIdQuery first, object second, bool expectedResult, string reason)
        {
            // Act
            bool actualResult = first.Equals(second);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }
    }
}

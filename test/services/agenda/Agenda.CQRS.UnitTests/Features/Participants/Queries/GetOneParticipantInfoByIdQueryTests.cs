using Agenda.CQRS.Features.Participants.Queries;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            GetOneParticipantInfoByIdQuery instance = new GetOneParticipantInfoByIdQuery(participantId);

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
            Action action = () => new GetOneParticipantInfoByIdQuery(default);

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
                        new GetOneParticipantInfoByIdQuery(participantId),
                        new GetOneParticipantInfoByIdQuery(participantId),
                        true,
                        $"two different {nameof(GetOneParticipantInfoByIdQuery)} with same data"
                    };
                }
            }

        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void AreEquals(GetOneParticipantInfoByIdQuery first, object second, bool expectedResult, string reason)
        {
            // Act
            bool actualResult = first.Equals(second);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }

    }
}

namespace Agenda.CQRS.UnitTests.Features.Participants.Queries
{
    using Agenda.CQRS.Features.Participants.Queries;
    using Agenda.DTO;
    using Agenda.Ids;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using MediatR;

    using Optional;

    using System;
    using System.Collections.Generic;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [Feature("Agenda")]
    [UnitTest]
    public class GetPlanningByParticipantIdQueryTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public GetPlanningByParticipantIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> InvalidArgumentCases
        {
            get
            {
                yield return new object[]
                {
                    (participantId: default(AttendeeId), from : (DateTimeOffset)1.January(2019), to : (DateTimeOffset)31.January(2019)),
                    "Participant ID is not set"
                };

                yield return new object[]
                {
                    (participantId: AttendeeId.New(), from : (DateTimeOffset)1.January(2019), to : (DateTimeOffset)31.December(2018)),
                    "Period is not valid (startDate > endDate"
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentCases))]
        public void Ctor_Throws_ArgumentException((AttendeeId participantId, DateTimeOffset from, DateTimeOffset to) data, string reason)
        {
            _outputHelper.WriteLine($"Data : {data}");

            // Act
            Action action = () => new GetPlanningByAttendeeIdQuery(data.participantId, data.from, data.to);

            // Assert
            action.Should()
                .Throw<ArgumentException>(reason).Which
                .Message.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void IsQuery() => typeof(GetPlanningByAttendeeIdQuery).Should()
            .Implement<IRequest<Option<IEnumerable<AppointmentInfo>>>>();
    }
}

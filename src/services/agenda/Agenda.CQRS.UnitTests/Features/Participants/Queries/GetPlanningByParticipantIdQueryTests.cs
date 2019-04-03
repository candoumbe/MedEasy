using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DTO;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.DAL.Repositories;
using MediatR;
using Optional;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Participants.Queries
{
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
                    (participantId: default(Guid), from : (DateTimeOffset)1.January(2019), to : (DateTimeOffset)31.January(2019)),
                    "Participant ID is not set"
                };

                yield return new object[]
                {
                    (participantId: Guid.NewGuid(), from : (DateTimeOffset)1.January(2019), to : (DateTimeOffset)31.December(2018)),
                    "Period is not valid (startDate > endDate"
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentCases))]
        public void Ctor_Throws_ArgumentException((Guid participantId, DateTimeOffset from, DateTimeOffset to) data, string reason)
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

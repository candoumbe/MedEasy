using Agenda.CQRS.Features.Appointments.Commands;
using FluentAssertions;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Commands
{
    [UnitTest]
    [Feature("Agenda")]
    [Feature("Appointments")]
    public class RescheduleAppointmentCommandTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public RescheduleAppointmentCommandTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsCommand() => typeof(RescheduleAppointmentCommand).Should()
            .NotBeAbstract().And
            .BeDerivedFrom<CommandBase<Guid, (Guid appointmentId, DateTimeOffset start, DateTimeOffset end), ModifyCommandResult>>();
    }
}

namespace Agenda.CQRS.UnitTests.Features.Appointments.Commands
{
    using Agenda.CQRS.Features.Appointments.Commands;
    using Agenda.Ids;

    using FluentAssertions;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using NodaTime;

    using System;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Agenda")]
    [Feature("Appointments")]
    public class ChangeAppointmentDateCommandTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ChangeAppointmentDateCommandTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        [Fact]
        public void IsCommand() => typeof(ChangeAppointmentDateCommand).Should()
            .NotBeAbstract().And
            .BeDerivedFrom<CommandBase<Guid, (AppointmentId appointmentId, ZonedDateTime start, ZonedDateTime end), ModifyCommandResult>>();
    }
}

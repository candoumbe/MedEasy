namespace Agenda.CQRS.UnitTests.Features.Appointments.Commands
{
    using Agenda.CQRS.Features.Appointments.Commands;
    using Agenda.Ids;

    using FluentAssertions;

    using System;

    using Xunit;
    using Xunit.Categories;

    [Feature("Agenda")]
    [UnitTest]
    public class DeleteAppointmentInfoByIdCommandTests
    {
        [Fact]
        public void GivenEmptyGuid_Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new DeleteAppointmentInfoByIdCommand(AppointmentId.Empty);

            // Assert
            action.Should()
                .Throw<ArgumentException>();
        }
    }
}

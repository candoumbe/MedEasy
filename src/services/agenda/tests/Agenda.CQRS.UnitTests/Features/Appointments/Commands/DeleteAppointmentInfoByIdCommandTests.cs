using Agenda.CQRS.Features.Appointments.Commands;

using FluentAssertions;

using System;

using Xunit;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Commands
{
    [Feature("Agenda")]
    [UnitTest]
    public class DeleteAppointmentInfoByIdCommandTests
    {
        [Fact]
        public void GivenEmptyGuid_Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new DeleteAppointmentInfoByIdCommand(Guid.Empty);

            // Assert
            action.Should()
                .Throw<ArgumentException>();
        }
    }
}

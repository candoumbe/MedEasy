using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.CQRS.Features.Appointments.Queries;
using FluentAssertions;
using System;
using Xunit;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Queries
{
    [UnitTest]
    [Feature("Agenda")]
    public class SearchAppointmentInfoQueryTests
    {
        [Fact]
        public void GivenNullParameter_Ctor_ThrowsArgumentNullException()
        {
            // Act
            Action action = () => new SearchAppointmentInfoQuery(null);

            // Arrange
            action.Should()
                .Throw<ArgumentNullException>().And
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}

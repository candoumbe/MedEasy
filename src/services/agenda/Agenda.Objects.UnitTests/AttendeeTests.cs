using System;
using Xunit;
using Xunit.Categories;
using FluentAssertions;
namespace Agenda.Objects.UnitTests
{
    [Feature("Agenda")]
    [UnitTest]
    public class AttendeeTests
    {
        [Fact]
        public void GivenNullParameter_Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new Attendee(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void GivenNullParameter_NameSetter_Throws_ArgumentNullException()
        {
            // Arrange
            Attendee participant = new Attendee("Bruce");

            // Act
            Action action = () => participant.Name = null;

            // Assert
            action.Should()
                .Throw<ArgumentNullException>();
        }


        [Theory]
        [InlineData("bruce Wayne", "Bruce Wayne")]
        public void Ctor_Builds_ValidObject(string name, string expectedName)
        {
            // Act
            Attendee participant = new Attendee(name);

            // Assert
            participant.Name.Should()
                .Be(expectedName);
            participant.PhoneNumber.Should()
                .BeNull();
            participant.Email.Should()
                .BeNull();

            participant.Appointments.Should()
                .BeEmpty();
        }
    }
}

namespace Agenda.CQRS.UnitTests.Features.Appointments.Commmands
{
    using Agenda.DTO;
    using System;
    using FluentAssertions;
    using Xunit;
    using FluentAssertions.Extensions;
    using System.Collections.Generic;
    using Agenda.CQRS.Features.Appointments.Commands;
    using Xunit.Categories;
    using NodaTime.Extensions;

    [Feature("Agenda")]
    [UnitTest]
    public class CreateAppointmentInfoCommandTests : IDisposable
    {

        public void Dispose() { }

        [Fact]
        public void Ctor_Is_Valid()
        {
            CreateAppointmentInfoCommand instance = new(new NewAppointmentInfo()
            {
            });

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new CreateAppointmentInfoCommand(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new CreateAppointmentInfoCommand(new NewAppointmentInfo { Location = "Wayne Tower", StartDate = 10.April(2010).Add(12.Hours()).AsUtc().ToInstant().InUtc(), EndDate = 10.April(2010).Add(13.Hours()).AsUtc().ToInstant().InUtc(), Subject = "Classified"  }),
                    new CreateAppointmentInfoCommand(new NewAppointmentInfo { Location = "Wayne Tower", StartDate = 10.April(2010).Add(12.Hours()).AsUtc().ToInstant().InUtc(), EndDate = 10.April(2010).Add(13.Hours()).AsUtc().ToInstant().InUtc(), Subject = "Classified"  }),
                    true,
                    "two commands with same data"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void AreEquals(CreateAppointmentInfoCommand first, CreateAppointmentInfoCommand second, bool expectedResult, string reason)
        {
            // Act
            bool actualResult = first.Equals(second);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }
    }
}

using Agenda.Ids;

using FluentAssertions;
using FluentAssertions.Extensions;

using NodaTime;
using NodaTime.Extensions;

using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.Objects.UnitTests
{
    [UnitTest]
    public class AppointmentTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public AppointmentTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void ChangingAppointment_Subject_ToNull_Throws_ArgumentNullException()
        {
            // Arrange
            Appointment attendee = new(id: AppointmentId.New(),
                                       subject: "JLA",
                                       location: "Wayne Manor",
                                       startDate: 12.July(2018).At(12.Hours()).AsUtc().ToInstant(),
                                       endDate: 12.July(2018).At(12.Hours().And(30.Minutes())).AsUtc().ToInstant());

            // Act
            Action action = () => attendee.ChangeSubjectTo(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>($"{nameof(Appointment)}'s {nameof(Appointment.Subject)} cannot be changed to null");
        }

        public static IEnumerable<object[]> ComputeStatusCases
        {
            get
            {
                Appointment appointment = new(
                    id: AppointmentId.New(),
                    subject: "Daily meeting",
                    location: "My office",
                    startDate: 12.April(2017).At(14.Hours()).AsUtc().ToInstant(),
                    endDate: 12.April(2017).At(17.Hours()).AsUtc().ToInstant());

                yield return new object[]
                {
                    appointment,
                    12.April(2017).At(12.Hours()).AsUtc().ToInstant(),
                    AppointmentStatus.NotStarted
                };
            }
        }

        [Theory]
        [MemberData(nameof(ComputeStatusCases))]
        public void ComputeStatus(Appointment appointment, Instant now, AppointmentStatus expected)
        {
            _outputHelper.WriteLine($"Appointment starts at {appointment.StartDate}");
            _outputHelper.WriteLine($"Appointment ends at {appointment.EndDate}");

            _outputHelper.WriteLine($"Date : {now}");

            // Act
            AppointmentStatus actual = appointment.Status;

            // Assert
            actual.Should()
                .Be(expected);

        }
    }
}

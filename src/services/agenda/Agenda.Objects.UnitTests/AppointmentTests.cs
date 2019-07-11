using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Agenda.Objects.UnitTests
{
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
            Appointment attendee = new Appointment(id : Guid.NewGuid(), subject :"JLA", location : "Wayne Manor", 12.July(2018).At(12.Hours()), 12.July(2018).At(12.Hours().And(30.Minutes())));

            // Act
            Action action = () => attendee.ChangeSubjectTo(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>($"Attendee's {nameof(Attendee.Name)} cannot be changed to null");
        }

        public static IEnumerable<object[]> ComputeStatusCases
        {
            get
            {
                Appointment appointment = new Appointment(
                    id: Guid.NewGuid(),
                    subject: "Daily meeting",
                    location: "My office",
                    startDate: 12.April(2017).At(14.Hours()),
                    endDate: 12.April(2017).At(17.Hours()));

                yield return new object[]
                {
                    appointment,
                    (DateTimeOffset)12.April(2017).At(12.Hours()),
                    AppointmentStatus.NotStarted
                };
            }
        }

        [Theory]
        [MemberData(nameof(ComputeStatusCases))]
        public void ComputeStatus(Appointment appointment, DateTimeOffset now, AppointmentStatus expected)
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

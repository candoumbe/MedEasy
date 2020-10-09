using Agenda.Domain.Events;

using AutoFixture.Xunit2;

using Bogus;

using FluentAssertions;
using FluentAssertions.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Xunit.Categories;

namespace Agenda.Domain.UnitTests
{
    [UnitTest]
    [Feature(nameof(Appointment))]
    [Feature(nameof(Agenda))]
    public class AppointmentTests
    {
        private readonly Faker _faker;

        public AppointmentTests()
        {
            _faker = new Faker();
        }

        [Theory]
        [AutoData]
        public void Given_validdata_Initialize_should_raise_AppointmentCreated(Guid appointmentId, string title, ISet<Attendee> attendees)
        {
            // Arrange
            DateTime start = 10.April(2010).Add(10.Hours());
            DateTime end = _faker.Date.Soon(refDate: start);

            Appointment appointment = new Appointment(appointmentId);

            // Act
            appointment.Initialize(start, end, title, attendees);

            // Assert
            appointment.Pending.Should()
                               .HaveCount(1).And
                               .ContainSingle(evt => evt is AppointmentCreated);

            AppointmentCreated appointmentCreatedEvent = appointment.Pending.OfType<AppointmentCreated>()
                                                                            .Single();

            appointmentCreatedEvent.Id.Should()
                                      .Be(appointmentId);
            appointmentCreatedEvent.Title.Should()
                                         .Be(title);
            appointmentCreatedEvent.Start.Should()
                                         .Be(start);
            appointmentCreatedEvent.End.Should()
                                       .Be(end);
            appointmentCreatedEvent.Attendees.Should()
                                             .BeEquivalentTo(attendees);
        }

        [Theory]
        [AutoData]
        public void Given_an_existing_appointment_Reschedule_should_raise_AppointmentRescheduled(ISet<Attendee> attendees)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            Appointment appointment = new Appointment(id);
            DateTime start = 10.January(2013).Add(15.Hours().Add(15.Minutes()));
            DateTime end = 10.January(2013).Add(15.Hours().Add(30.Minutes()));

            appointment.Initialize(start, end, _faker.Lorem.Sentence(), attendees);

            int offset = _faker.Random.Int(min: 1, max: 31);

            DateTime newStart = start.AddDays(offset);
            DateTime newEnd = end.AddDays(offset);

            // Act
            appointment.Reschedule(newStart, newEnd);

            // Assert
            appointment.Pending.Should()
                               .ContainSingle(evt => evt is AppointmentRescheduled);

            AppointmentRescheduled evt = appointment.Pending.OfType<AppointmentRescheduled>()
                                                            .Single();

            evt.Id.Should()
                  .Be(id);
            evt.Start.Should()
                     .Be(newStart);
            evt.End.Should()
                   .Be(newEnd);
        }

        [Theory]
        [AutoData]
        public void Given_an_existing_appointment_Cancel_should_raise_AppointmentCancelled_For_Each_Participant(ISet<Attendee> attendees, Guid cancellatorId)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            Appointment appointment = new Appointment(id);
            DateTime start = 10.January(2013).Add(15.Hours().Add(15.Minutes()));
            DateTime end = 10.January(2013).Add(15.Hours().Add(30.Minutes()));

            appointment.Initialize(start, end, _faker.Lorem.Sentence(), attendees);

            int offset = _faker.Random.Int(min: 1, max: 31);

            DateTime newStart = start.AddDays(offset);
            DateTime newEnd = end.AddDays(offset);

            // Act
            appointment.Cancel(by : cancellatorId);

            // Assert
            appointment.Pending.Should()
                               .HaveCount(1).And
                               .ContainSingle(evt => evt is AppointmentCancelled);

            AppointmentCancelled evt = appointment.Pending.OfType<AppointmentCancelled>()
                                                            .Single();

            evt.Id.Should()
                  .Be(appointment.Id);
        }
    }
}

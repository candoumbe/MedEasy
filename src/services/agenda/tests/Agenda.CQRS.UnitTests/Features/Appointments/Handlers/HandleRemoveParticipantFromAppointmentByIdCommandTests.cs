using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.DataStores;
using Agenda.Ids;
using Agenda.Objects;

using FluentAssertions;
using FluentAssertions.Extensions;

using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using Microsoft.EntityFrameworkCore;

using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;

using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleRemoveAttendeeFromAppointmentByIdCommandTests : IClassFixture<SqliteEfCoreDatabaseFixture<AgendaContext>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly HandleRemoveAttendeeFromAppointmentByIdCommand _sut;

        public HandleRemoveAttendeeFromAppointmentByIdCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<AgendaContext> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(database.OptionsBuilder.Options, (options) =>
            {
                AgendaContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleRemoveAttendeeFromAppointmentByIdCommand(_uowFactory);
        }

        [Fact]
        public async Task GivenNoAppointment_Handles_Returns_NotFound()
        {
            // Arrange
            AttendeeId participantId = AttendeeId.New();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>()
                   .Create(new Attendee(id: participantId, "Dick Grayson"));
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (AppointmentId appointmentId, AttendeeId participantId) data = (appointmentId: AppointmentId.New(), participantId);

            // Act
            DeleteCommandResult cmdResult = await _sut.Handle(new RemoveAttendeeFromAppointmentByIdCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(DeleteCommandResult.Failed_NotFound);
        }

        [Fact]
        public async Task GivenNoAttendee_Handles_Returns_NotFound()
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();
            Appointment appointment = new(
                id: appointmentId,
                startDate: 17.July(2016).At(13.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                endDate: 17.July(2016).At(13.Hours().And(45.Minutes())).AsUtc().ToInstant(),
                subject: "Confidential",
                location: "Somewhere in Gotham"
            );
            appointment.AddAttendee(new Attendee(id: AttendeeId.New(), name: "Dick Grayson"));

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                         .ConfigureAwait(false);
            }
            (AppointmentId appointmentId, AttendeeId participantId) data = (appointmentId, participantId: AttendeeId.New());

            // Act
            DeleteCommandResult cmdResult = await _sut.Handle(new RemoveAttendeeFromAppointmentByIdCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(DeleteCommandResult.Failed_NotFound);
        }

        [Fact]
        public async Task Given_appointment_and_attendees_exist_Handle_should_returns_Done()
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();
            Appointment appointment = new(id: appointmentId,
                                          startDate: 17.July(2016).At(13.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                                          endDate: 17.July(2016).At(13.Hours().And(45.Minutes())).AsUtc().ToInstant(),
                                          subject: "Confidential",
                                          location: "Somewhere in Gotham");

            Attendee dickGrayson = new(id: AttendeeId.New(), name: "Dick Grayson");
            appointment.AddAttendee(dickGrayson);

            Attendee bruceWayne = new(id: AttendeeId.New(), name: "Bruce Wayne");
            appointment.AddAttendee(bruceWayne);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (AppointmentId appointmentId, AttendeeId attendeeId) data = (appointmentId, attendeeId: bruceWayne.Id);

            // Act
            DeleteCommandResult cmdResult = await _sut.Handle(new RemoveAttendeeFromAppointmentByIdCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                     .Be(DeleteCommandResult.Done);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool attendeeRemoved = !await uow.Repository<Appointment>()
                                                 .AnyAsync(x => x.Id == data.appointmentId && x.Attendees.Any(attendee => attendee.Id == data.attendeeId))
                                                 .ConfigureAwait(false);

                attendeeRemoved.Should()
                    .BeTrue("the relation should no longer exists in the datastore");
            }
        }
    }
}

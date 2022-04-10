namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
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

    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [Feature("Agenda")]
    [UnitTest]
    public class HandleAddParticipantToAppointmentCommandTests : IClassFixture<SqliteEfCoreDatabaseFixture<AgendaContext>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly HandleAddParticipantToAppointmentCommand _sut;

        public HandleAddParticipantToAppointmentCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<AgendaContext> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(database.OptionsBuilder.Options, (options) =>
            {
                AgendaContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleAddParticipantToAppointmentCommand(_uowFactory);
        }


        [Fact]
        public async Task GivenNoAppointment_Handles_Returns_NotFound()
        {
            // Arrange
            AttendeeId participantId = AttendeeId.New();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Create(new Attendee(id: participantId, "Dick Grayson"));
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            (AppointmentId appointmentId, AttendeeId participantId) data = (appointmentId: AppointmentId.New(), participantId);

            // Act
            ModifyCommandResult cmdResult = await _sut.Handle(new AddAttendeeToAppointmentCommand(data), default)
                                                      .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                     .Be(ModifyCommandResult.Failed_NotFound);
        }

        [Fact]
        public async Task GivenNoParticipant_Handles_Returns_NotFound()
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
            ModifyCommandResult cmdResult = await _sut.Handle(new AddAttendeeToAppointmentCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(ModifyCommandResult.Failed_NotFound);
        }

        [Fact]
        public async Task GivenAppointmentAlreadyHasSameParticipant_Handle_Returns_Conflict()
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();
            Appointment appointment = new(id: appointmentId,
                                          subject: "Confidential",
                                          location: "Somewhere in Gotham",
                                          startDate: 17.July(2016).At(13.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                                          endDate: 17.July(2016).At(13.Hours().And(45.Minutes())).AsUtc().ToInstant());

            Attendee participant = new(id: AttendeeId.New(), name: "Dick Grayson");
            appointment.AddAttendee(participant);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (AppointmentId appointmentId, AttendeeId participantId) data = (appointmentId, participantId: participant.Id);

            // Act
            ModifyCommandResult cmdResult = await _sut.Handle(new AddAttendeeToAppointmentCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(ModifyCommandResult.Failed_Conflict, "the participant is already associated to the appointement");
        }

        [Fact]
        public async Task GivenAppointmentDoesNotAlreadyHaveSameParticipant_Handle_Returns_Done()
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();
            Appointment appointment = new(id: appointmentId,
                                           startDate: 17.July(2016).At(13.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                                           endDate: 17.July(2016).Add(13.Hours().And(45.Minutes())).AsUtc().ToInstant(),
                                           subject: "Confidential", location: "Somewhere in Gotham");

            Attendee dickGrayson = new(id: AttendeeId.New(), name: "Dick Grayson");
            Attendee bruceWayne = new(id: AttendeeId.New(), name: "Bruce Wayne");

            appointment.AddAttendee(dickGrayson);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                uow.Repository<Attendee>().Create(bruceWayne);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (AppointmentId appointmentId, AttendeeId attendeeId) data = (appointmentId, attendeeId: bruceWayne.Id);
            // Act
            ModifyCommandResult cmdResult = await _sut.Handle(new AddAttendeeToAppointmentCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(ModifyCommandResult.Done, "the participant is not already associated to the appointement but he exists in the datastore");
        }
    }
}

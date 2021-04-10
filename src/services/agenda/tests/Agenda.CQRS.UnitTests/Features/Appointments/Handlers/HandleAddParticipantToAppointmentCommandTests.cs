using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.DataStores;
using Agenda.Objects;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;

using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleAddParticipantToAppointmentCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private HandleAddParticipantToAppointmentCommand _sut;
        private DatabaseFacade _databaseFacade;

        public HandleAddParticipantToAppointmentCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<AgendaContext> builder = new();
            builder = builder.UseInMemoryDatabase($"{Guid.NewGuid()}")
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(warnings => warnings.Throw());

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(builder.Options, (options) =>
            {
                AgendaContext context = new(options, new FakeClock(new Instant()));
                _databaseFacade = context.Database;
                _databaseFacade.EnsureCreated();
                return context;
            });
            _sut = new HandleAddParticipantToAppointmentCommand(_uowFactory);
        }

        public async void Dispose()
        {
            await _databaseFacade?.EnsureDeletedAsync();

            _sut = null;
        }

        [Fact]
        public async Task GivenNoAppointment_Handles_Returns_NotFound()
        {
            // Arrange
            Guid participantId = Guid.NewGuid();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Create(new Attendee(id: participantId, "Dick Grayson"));
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid participantId) data = (appointmentId: Guid.NewGuid(), participantId);

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
            Guid appointmentId = Guid.NewGuid();
            Appointment appointment = new            (
                id: appointmentId,
                startDate : 17.July(2016).At(13.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                endDate : 17.July(2016).At(13.Hours().And(45.Minutes())).AsUtc().ToInstant(),
                subject : "Confidential",
                location : "Somewhere in Gotham"

            );
            appointment.AddAttendee(new Attendee(id: Guid.NewGuid(), name: "Dick Grayson"));

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid participantId) data = (appointmentId, participantId: Guid.NewGuid());

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
            Guid appointmentId = Guid.NewGuid();
            Appointment appointment = new            (
                id: appointmentId,
                startDate: 17.July(2016).At(13.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                endDate: 17.July(2016).At(13.Hours().And(45.Minutes())).AsUtc().ToInstant(),
                subject: "Confidential",
                location: "Somewhere in Gotham"

            );
            Attendee participant = new(id: Guid.NewGuid(), name: "Dick Grayson");
            appointment.AddAttendee(participant);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid participantId) data = (appointmentId, participantId: participant.Id);
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
            Guid appointmentId = Guid.NewGuid();
            Appointment appointment = new (id: appointmentId,
                                           startDate: 17.July(2016).At(13.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                                           endDate: 17.July(2016).Add(13.Hours().And(45.Minutes())).AsUtc().ToInstant(),
                                           subject: "Confidential", location: "Somewhere in Gotham");

            Attendee dickGrayson = new(id: Guid.NewGuid(), name:"Dick Grayson");
            Attendee bruceWayne = new(id: Guid.NewGuid(), name:"Bruce Wayne");

            appointment.AddAttendee(dickGrayson);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                uow.Repository<Attendee>().Create(bruceWayne);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid attendeeId) data = (appointmentId, attendeeId: bruceWayne.Id);
            // Act
            ModifyCommandResult cmdResult = await _sut.Handle(new AddAttendeeToAppointmentCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(ModifyCommandResult.Done, "the participant is not already associated to the appointement but he exists in the datastore");
        }
    }
}

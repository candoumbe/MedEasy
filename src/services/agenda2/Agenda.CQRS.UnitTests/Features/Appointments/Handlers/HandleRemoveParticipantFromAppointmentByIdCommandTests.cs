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
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleRemoveAttendeeFromAppointmentByIdCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private HandleRemoveAttendeeFromAppointmentByIdCommand _sut;
        private DatabaseFacade _databaseFacade;

        public HandleRemoveAttendeeFromAppointmentByIdCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<AgendaContext> builder = new DbContextOptionsBuilder<AgendaContext>();
            builder.UseSqlite(database.Connection)
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(warnings => warnings.Throw());

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(builder.Options, (options) =>
            {
                AgendaContext context = new AgendaContext(options);
                _databaseFacade = context.Database;
                _databaseFacade.EnsureCreated();
                return context;
            });
            _sut = new HandleRemoveAttendeeFromAppointmentByIdCommand(_uowFactory);
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
                uow.Repository<Attendee>().Create(new Attendee(id : participantId,"Dick Grayson"));
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid participantId) data = (appointmentId: Guid.NewGuid(), participantId);

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
            Guid appointmentId = Guid.NewGuid();
            Appointment appointment = new Appointment
            (
                id: appointmentId,
                startDate: 17.July(2016).At(13.Hours().And(30.Minutes())),
                endDate: 17.July(2016).At(13.Hours().And(45.Minutes())),
                subject: "Confidential",
                location: "Somewhere in Gotham"
            );
            appointment.AddAttendee(new Attendee(id: Guid.NewGuid(), name:"Dick Grayson"));

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid participantId) data = (appointmentId, participantId: Guid.NewGuid());

            // Act
            DeleteCommandResult cmdResult = await _sut.Handle(new RemoveAttendeeFromAppointmentByIdCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(DeleteCommandResult.Failed_NotFound);
        }

        [Fact]
        public async Task GivenAppointmentAndAttendeeExist_Handle_ReturnsDone()
        {
            // Arrange
            Guid appointmentId = Guid.NewGuid();
            Appointment appointment = new Appointment
            (
                id: appointmentId,
                startDate: 17.July(2016).At(13.Hours().And(30.Minutes())),
                endDate: 17.July(2016).At(13.Hours().And(45.Minutes())),
                subject: "Confidential",
                location: "Somewhere in Gotham"

            );
            Attendee dickGrayson = new Attendee(id: Guid.NewGuid(), name:"Dick Grayson");
            appointment.AddAttendee(dickGrayson);

            Attendee bruceWayne = new Attendee(id: Guid.NewGuid(), name: "Bruce Wayne");
            appointment.AddAttendee(bruceWayne);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid attendeeId) data = (appointmentId, attendeeId: bruceWayne.Id);

            // Act
            DeleteCommandResult cmdResult = await _sut.Handle(new RemoveAttendeeFromAppointmentByIdCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(DeleteCommandResult.Done);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool attendeeRemoved = !await uow.Repository<AppointmentAttendee>()
                    .AnyAsync(x => x.AppointmentId == data.appointmentId && x.AttendeeId == data.attendeeId)
                    .ConfigureAwait(false);

                attendeeRemoved.Should()
                    .BeTrue("the relation should no longer exists in the datastore");
            }
        }
    }
}

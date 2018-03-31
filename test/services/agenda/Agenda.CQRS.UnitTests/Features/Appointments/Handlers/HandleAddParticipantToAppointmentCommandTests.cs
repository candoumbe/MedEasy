using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.DataStores;
using Agenda.Objects;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Context;
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
    public class HandleAddParticipantToAppointmentCommandTests : IDisposable, IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private HandleAddParticipantToAppointmentCommand _sut;
        private DatabaseFacade _databaseFacade;

        public HandleAddParticipantToAppointmentCommandTests(ITestOutputHelper outputHelper, DatabaseFixture database)
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
                uow.Repository<Participant>().Create(new Participant("Dick Grayson") { UUID = participantId });
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid participantId) data = (appointmentId: Guid.NewGuid(), participantId);

            // Act
            ModifyCommandResult cmdResult = await _sut.Handle(new AddParticipantToAppointmentCommand(data), default)
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
            Appointment appointment = new Appointment
            {
                UUID = appointmentId,
                StartDate = 17.July(2016).Add(13.Hours().Add(30.Minutes())),
                EndDate = 17.July(2016).Add(13.Hours().Add(45.Minutes())),
                Subject = "Confidential",
                Location = "Somewhere in Gotham"

            };
            appointment.AddParticipant(new Participant("Dick Grayson") { UUID = Guid.NewGuid() });

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid participantId) data = (appointmentId, participantId: Guid.NewGuid());

            // Act
            ModifyCommandResult cmdResult = await _sut.Handle(new AddParticipantToAppointmentCommand(data), default)
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
            Appointment appointment = new Appointment
            {
                UUID = appointmentId,
                StartDate = 17.July(2016).Add(13.Hours().Add(30.Minutes())),
                EndDate = 17.July(2016).Add(13.Hours().Add(45.Minutes())),
                Subject = "Confidential",
                Location = "Somewhere in Gotham"

            };

            Participant participant = new Participant("Dick Grayson") { UUID = Guid.NewGuid() };
            appointment.AddParticipant(participant);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid participantId) data = (appointmentId, participantId: participant.UUID);
            // Act
            ModifyCommandResult cmdResult = await _sut.Handle(new AddParticipantToAppointmentCommand(data), default)
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
            Appointment appointment = new Appointment
            {
                UUID = appointmentId,
                StartDate = 17.July(2016).Add(13.Hours().Add(30.Minutes())),
                EndDate = 17.July(2016).Add(13.Hours().Add(45.Minutes())),
                Subject = "Confidential",
                Location = "Somewhere in Gotham"

            };

            Participant dickGrayson = new Participant("Dick Grayson") { UUID = Guid.NewGuid() };
            Participant bruceWayne = new Participant("Bruce Wayne") { UUID = Guid.NewGuid() };
            appointment.AddParticipant(dickGrayson);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                uow.Repository<Participant>().Create(bruceWayne);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            (Guid appointmentId, Guid participantId) data = (appointmentId, participantId: bruceWayne.UUID);
            // Act
            ModifyCommandResult cmdResult = await _sut.Handle(new AddParticipantToAppointmentCommand(data), default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(ModifyCommandResult.Done, "the participant is not already associated to the appointement but he exists in the datastore");

        }
    }
}

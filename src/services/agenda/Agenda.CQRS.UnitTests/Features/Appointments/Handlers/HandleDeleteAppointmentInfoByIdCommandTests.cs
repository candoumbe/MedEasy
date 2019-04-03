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
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleDeleteAppointmentInfoByIdCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private HandleDeleteAppointmentInfoByIdCommand _sut;

        public HandleDeleteAppointmentInfoByIdCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;
            DbContextOptionsBuilder<AgendaContext> builder = new DbContextOptionsBuilder<AgendaContext>();
            builder.UseSqlite(database.Connection)
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(warnings => warnings.Throw());

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(builder.Options, (options) =>
            {
                AgendaContext context = new AgendaContext(options);
               context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleDeleteAppointmentInfoByIdCommand(_uowFactory);
        }

        public void Dispose()
        {
            _uowFactory = null;
            _sut = null;
        }

        [Fact]
        public void GivenNullParameter_Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new HandleDeleteAppointmentInfoByIdCommand(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task GivenRecordExists_Handle_Returns_DeleteOk()
        {
            // Arrange
            Guid appointmentUuid = Guid.NewGuid();
            Appointment appointment = new Appointment
            {
                UUID = appointmentUuid,
                StartDate = 16.July(2016).Add(15.Hours().Add(30.Minutes())),
                EndDate = 16.July(2016).Add(15.Hours().Add(45.Minutes())),
                Subject = "Confidential",
                Location = "Wayne Tower"
            };

            Guid firstParticipantId = Guid.NewGuid();
            Guid secondParticipantId = Guid.NewGuid();
            appointment.AddAttendee(new Attendee ("Dick Grayson"){ UUID = firstParticipantId });
            appointment.AddAttendee(new Attendee ("Bruce Wayne") { UUID = secondParticipantId });

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            DeleteAppointmentInfoByIdCommand cmd = new DeleteAppointmentInfoByIdCommand(appointmentUuid);
            // Act
            DeleteCommandResult cmdResult = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(DeleteCommandResult.Done);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool deleteOk = !await uow.Repository<Appointment>().AnyAsync(x => x.UUID == appointmentUuid);

                deleteOk.Should()
                    .BeTrue("deleted resource must not be prensent in the datastore");

                bool participantsNotDeleted = await uow.Repository<Attendee>()
                    .AnyAsync(x => new[] { firstParticipantId, secondParticipantId }.Contains(x.UUID))
                    .ConfigureAwait(false);

                participantsNotDeleted.Should()
                    .BeTrue("deleting an appointment must not remove associated participants from the datastore");
            }
        }
    }
}

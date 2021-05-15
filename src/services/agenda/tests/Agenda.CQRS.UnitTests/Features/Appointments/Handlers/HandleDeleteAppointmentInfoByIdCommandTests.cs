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

    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [Feature("Agenda")]
    [Feature("Appointments")]
    [UnitTest]
    public class HandleDeleteAppointmentInfoByIdCommandTests : IClassFixture<SqliteEfCoreDatabaseFixture<AgendaContext>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly HandleDeleteAppointmentInfoByIdCommand _sut;

        public HandleDeleteAppointmentInfoByIdCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<AgendaContext> database)
        {
            _outputHelper = outputHelper;
            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(database.OptionsBuilder.Options, (options) =>
            {
                AgendaContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleDeleteAppointmentInfoByIdCommand(_uowFactory);
        }

        [Fact]
        public void Given_null_parameter_ctor_throws_ArgumentNullException()
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
            AppointmentId appointmentUuid = AppointmentId.New();
            Appointment appointment = new(
                id: appointmentUuid,
                startDate: 16.July(2016).At(15.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                endDate: 16.July(2016).At(15.Hours().And(45.Minutes())).AsUtc().ToInstant(),
                subject: "Confidential",
                location: "Wayne Tower"
            );

            AttendeeId firstParticipantId = AttendeeId.New();
            AttendeeId secondParticipantId = AttendeeId.New();

            appointment.AddAttendee(new Attendee(id: firstParticipantId, name: "Dick Grayson"));
            appointment.AddAttendee(new Attendee(id: secondParticipantId, name: "Bruce Wayne"));

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            DeleteAppointmentInfoByIdCommand cmd = new(appointmentUuid);
            // Act
            DeleteCommandResult cmdResult = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(DeleteCommandResult.Done);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool deleteOk = !await uow.Repository<Appointment>().AnyAsync(x => x.Id == appointmentUuid)
                    .ConfigureAwait(false);

                deleteOk.Should()
                    .BeTrue("deleted resource must not be prensent in the datastore");

                bool participantsNotDeleted = await uow.Repository<Attendee>()
                    .AnyAsync(x => new[] { firstParticipantId, secondParticipantId }.Contains(x.Id))
                    .ConfigureAwait(false);

                participantsNotDeleted.Should()
                    .BeTrue("deleting an appointment must not remove associated participants from the datastore");
            }
        }
    }
}

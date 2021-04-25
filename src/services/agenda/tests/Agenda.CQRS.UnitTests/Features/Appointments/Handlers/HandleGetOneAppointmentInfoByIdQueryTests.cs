using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.Ids;
using Agenda.Mapping;
using Agenda.Objects;

using AutoMapper;

using FluentAssertions;
using FluentAssertions.Extensions;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;

using Optional;

using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleGetOneAppointmentInfoByIdQueryTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<AgendaContext>>
    {
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;
        private HandleGetOneAppointmentInfoByIdQuery _sut;

        public HandleGetOneAppointmentInfoByIdQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<AgendaContext> database)
        {
            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(database.OptionsBuilder.Options, (options) =>
            {
                AgendaContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _sut = new HandleGetOneAppointmentInfoByIdQuery(_uowFactory, _mapper);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Clear();
                uow.Repository<Appointment>().Clear();

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _uowFactory = null;
            _mapper = null;
            _sut = null;
        }

        [Fact]
        public async Task GivenEmptyDataStore_Handle_Returns_None()
        {
            // Arrange
            GetOneAppointmentInfoByIdQuery request = new(AppointmentId.New());

            // Act
            Option<AppointmentInfo> optionalAppointment = await _sut.Handle(request, ct: default)
                .ConfigureAwait(false);

            // Assert
            optionalAppointment.HasValue.Should().BeFalse();
        }

        [Fact]
        public async Task GivenRecordExistsInDataStore_Get_Returns_Some()
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();
            Appointment appointment = new(
                id: appointmentId,
                location: "Wayne Tower",
                subject: "Contengency",
                startDate: 1.April(2018).Add(15.Hours()).AsUtc().ToInstant(),
                endDate: 1.April(2018).Add(16.Hours()).AsUtc().ToInstant()
            );
            Attendee bruce = new(id: AttendeeId.New(), name: "Bruce Wayne");
            appointment.AddAttendee(bruce);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);

                await uow.SaveChangesAsync()
                         .ConfigureAwait(false);
            }

            GetOneAppointmentInfoByIdQuery request = new(appointmentId);

            // Act
            Option<AppointmentInfo> optionalAppointment = await _sut.Handle(request, default)
                                                                    .ConfigureAwait(false);

            // Assert
            optionalAppointment.HasValue.Should().BeTrue($"the record <{appointmentId}> exists in the datastore");
            optionalAppointment.MatchSome((appointmentInfo) =>
            {
                appointmentInfo.Id.Should().Be(appointment.Id);
                appointmentInfo.Location.Should().Be(appointment.Location);
                appointmentInfo.Subject.Should().Be(appointment.Subject);
                appointmentInfo.StartDate.Should().Be(appointment.StartDate);
                appointmentInfo.EndDate.Should().Be(appointment.EndDate);
                appointmentInfo.Attendees.Should()
                                         .HaveSameCount(appointment.Attendees);

                AttendeeInfo attendeeInfo = appointmentInfo.Attendees.ElementAt(0);
                attendeeInfo.Id.Should().Be(bruce.Id);
                attendeeInfo.Name.Should().Be(appointment.Attendees.ElementAt(0).Name);
                attendeeInfo.UpdatedDate.Should()
                    .NotBe(Instant.MinValue).And
                    .NotBe(Instant.MaxValue);
            });
        }
    }
}

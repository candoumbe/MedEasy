namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    using Agenda.CQRS.Features.Appointments.Commands;
    using Agenda.CQRS.Features.Appointments.Handlers;
    using Agenda.DataStores;
    using Agenda.Ids;
    using Agenda.Mapping;
    using Agenda.Objects;

    using AutoMapper;

    using Bogus;

    using FakeItEasy;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.CQRS.Core.Exceptions;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

    using Microsoft.EntityFrameworkCore;

    using NodaTime;
    using NodaTime.Extensions;
    using NodaTime.Testing;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Agenda")]
    [Feature("Appointments")]
    public class HandleChangeAppointmentDateCommandTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<AgendaContext>>
    {
        private HandleChangeAppointmentDateCommand _sut;
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactoryMock;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IMapper _mapper;

        public HandleChangeAppointmentDateCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<AgendaContext> database)
        {
            _outputHelper = outputHelper;
            _unitOfWorkFactory = new EFUnitOfWorkFactory<AgendaContext>(database.OptionsBuilder.Options, (options) =>
            {
                AgendaContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();

                return context;
            });
            _mapper = A.Fake<IMapper>(x => x.Wrapping(AutoMapperConfig.Build().CreateMapper()));
            _unitOfWorkFactoryMock = A.Fake<IUnitOfWorkFactory>(x => x.Wrapping(_unitOfWorkFactory));

            _sut = new HandleChangeAppointmentDateCommand(_unitOfWorkFactoryMock, _mapper);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using (IUnitOfWork uow = _unitOfWorkFactoryMock.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Clear();
                uow.Repository<Appointment>().Clear();

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _unitOfWorkFactoryMock = null;
            _unitOfWorkFactory = null;
            _sut = null;
        }

        [Fact]
        public void Handle_ChangeAppointmentDateCommand() => typeof(HandleChangeAppointmentDateCommand).Should()
            .Implement<IRequestHandler<ChangeAppointmentDateCommand, ModifyCommandResult>>();

        public static IEnumerable<object[]> InvalidDataCommandCases
        {
            get
            {
                AppointmentId[] appointmentIds = { AppointmentId.New(), default };
                ZonedDateTime[] starts = { 23.February(2017).Add(15.Hours()).AsUtc().ToInstant().InUtc(), default };
                ZonedDateTime[] ends = { 23.February(2017).Add(15.Hours().And(15.Minutes())).AsUtc().ToInstant().InUtc(), default };

                IEnumerable<object[]> cases = appointmentIds
                    .CrossJoin(starts, (appointmentId, start) => (appointmentId, start))
                    .CrossJoin(ends, (tuple, end) => (tuple.appointmentId, tuple.start, end))
                    .Where(tuple => !tuple.Equals((default, default, default)) && (tuple.appointmentId == default || tuple.start == default || tuple.end == default))
                    .Select(tuple => new object[] { tuple, "One or more properties are not set" });

                foreach (object[] @case in cases)
                {
                    yield return @case;
                }

                yield return new object[] { (appointmentId: AppointmentId.New(), start: 17.August(2007).AsUtc().ToInstant().InUtc(), end: 17.August(2007).Add(-1.Hours()).AsUtc().ToInstant().InUtc()), "Start property is after end" };
            }
        }

        public static IEnumerable<object[]> HandleCases
        {
            get
            {
                Faker faker = new();
                yield return new object[]
                {
                    Enumerable.Empty<Appointment>(),
                    new ChangeAppointmentDateCommand((AppointmentId.New(), start: 17.February(2012).Add(13.Hours()).AsUtc().ToInstant().InUtc(), end: 17.February(2012).Add(13.Hours().And(15.Minutes())).AsUtc().ToInstant().InUtc())),
                    ModifyCommandResult.Failed_NotFound,
                    "Appointment not found in the datastore"
                };
                {
                    Appointment appointment = new(id: AppointmentId.New(),
                                                   subject: "JLA relocation",
                                                   location: "None",
                                                   startDate: 25.April(2012).At(14.Hours()).AsUtc().ToInstant(),
                                                   endDate: 25.April(2012).At(14.Hours().And(15.Minutes())).AsUtc().ToInstant());

                    Attendee batman = new(id: AttendeeId.New(), name: "Bruce Wayne");
                    Attendee superman = new(id: AttendeeId.New(), name: "Clark Kent");

                    appointment.AddAttendee(batman);
                    appointment.AddAttendee(superman);

                    yield return new object[]
                    {
                        new []{ appointment },
                        new ChangeAppointmentDateCommand((appointment.Id, start: 17.February(2012).Add(13.Hours()).AsUtc().ToInstant().InUtc(), end: 17.February(2012).Add(13.Hours().And(15.Minutes())).AsUtc().ToInstant().InUtc())),
                        ModifyCommandResult.Done,
                        "The appointment exists and the change won't overlap with any existing appointment"
                    };
                }

                {
                    Appointment appointmentRelocation = new(
                        id: AppointmentId.New(),
                        startDate: 25.April(2012).At(14.Hours()).AsUtc().ToInstant(),
                        endDate: 25.April(2012).At(14.Hours().And(15.Minutes())).AsUtc().ToInstant(),
                        subject: "JLA relocation",
                        location: "None"
                    );

                    Attendee batman = new(id: AttendeeId.New(), name: "Bruce Wayne");
                    Attendee superman = new(id: AttendeeId.New(), name: "Clark Kent");

                    appointmentRelocation.AddAttendee(batman);
                    appointmentRelocation.AddAttendee(superman);

                    Appointment appointmentEmancipation = new(
                        id: AppointmentId.New(),
                        startDate: 25.April(2012).At(13.Hours()).AsUtc().ToInstant(),
                        endDate: 25.April(2012).At(14.Hours().And(5.Minutes())).AsUtc().ToInstant(),
                        subject: "I want to leave the JLA",
                        location: "None"
                    );

                    Attendee robin = new(id: AttendeeId.New(), name: "Dick grayson");

                    appointmentEmancipation.AddAttendee(batman);
                    appointmentEmancipation.AddAttendee(robin);

                    yield return new object[]
                    {
                        new []{ appointmentRelocation, appointmentEmancipation },
                        new ChangeAppointmentDateCommand((appointmentEmancipation.Id, start: faker.Noda().ZonedDateTime.Past(reference: appointmentRelocation.StartDate.InUtc()), end: faker.Noda().ZonedDateTime.Between(appointmentRelocation.StartDate.InUtc(), appointmentRelocation.EndDate.InUtc()))),
                        ModifyCommandResult.Failed_Conflict,
                        "The appointment would end when another appointemtn is ongoing"
                    };
                }

                {
                    Appointment appointmentRelocation = new(
                        id: AppointmentId.New(),
                        startDate: 25.April(2012).At(14.Hours()).AsUtc().ToInstant(),
                        endDate: 25.April(2012).At(14.Hours().And(15.Minutes())).AsUtc().ToInstant(),
                        subject: "JLA relocation",
                        location: "None"
                    );

                    Attendee batman = new(id: AttendeeId.New(), name: "Bruce Wayne");
                    Attendee superman = new(id: AttendeeId.New(), name: "Clark Kent");

                    appointmentRelocation.AddAttendee(batman);
                    appointmentRelocation.AddAttendee(superman);

                    Appointment appointmentEmancipation = new(
                        id: AppointmentId.New(),
                        startDate: 25.April(2012).At(13.Hours()).AsUtc().ToInstant(),
                        endDate: 25.April(2012).At(14.Hours().And(5.Minutes())).AsUtc().ToInstant(),
                        subject: "I want to leave the JLA",
                        location: "None"
                    );

                    Attendee robin = new(id: AttendeeId.New(), "Dick grayson");

                    appointmentEmancipation.AddAttendee(batman);
                    appointmentEmancipation.AddAttendee(robin);

                    yield return new object[]
                    {
                        new []{ appointmentRelocation, appointmentEmancipation },
                        new ChangeAppointmentDateCommand((appointmentEmancipation.Id, start: faker.Noda().ZonedDateTime.Between(appointmentRelocation.StartDate.InUtc(), appointmentRelocation.EndDate.InUtc()), end: faker.Noda().ZonedDateTime.Future(reference : appointmentRelocation.EndDate.InUtc()))),
                        ModifyCommandResult.Failed_Conflict,
                        "The appointment would start whilst another appointment is ongoing"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(HandleCases))]
        public async Task HandleTests(IEnumerable<Appointment> appointments, ChangeAppointmentDateCommand cmd, ModifyCommandResult expected, string reason)
        {
            // Arrange
            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointments);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            // Act
            ModifyCommandResult actual = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            A.CallTo(() => _unitOfWorkFactoryMock
                .NewUnitOfWork())
                .MustHaveHappenedOnceExactly();
            A.CallTo(_mapper).MustNotHaveHappened();

            actual.Should()
                .Be(expected, reason);

            if (actual == ModifyCommandResult.Done)
            {
                using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();
                (AppointmentId appointmentId, ZonedDateTime start, ZonedDateTime end) = cmd.Data;
                bool changesOk = await uow.Repository<Appointment>()
                                          .AnyAsync(app => app.Id == appointmentId
                                                           && app.StartDate == start.ToInstant()
                                                           && app.EndDate == end.ToInstant())
                                          .ConfigureAwait(false);

                changesOk.Should()
                         .BeTrue("Changes must reflect in the datastore");
            }
        }
    }
}

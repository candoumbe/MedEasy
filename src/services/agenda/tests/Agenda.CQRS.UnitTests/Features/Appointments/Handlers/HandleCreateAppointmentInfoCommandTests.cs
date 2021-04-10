using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.Mapping;
using Agenda.Objects;

using AutoMapper;

using Bogus;

using FakeItEasy;

using FluentAssertions;
using FluentAssertions.Extensions;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

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

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleCreateAppointmentInfoCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IMapper _mapperMock;
        private HandleCreateAppointmentInfoCommand _sut;

        public HandleCreateAppointmentInfoCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<AgendaContext> dbContextOptionsBuilder = new();
            dbContextOptionsBuilder.UseInMemoryDatabase($"{Guid.NewGuid()}")
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(warnings => warnings.Throw());

            _unitOfWorkFactory = new EFUnitOfWorkFactory<AgendaContext>(dbContextOptionsBuilder.Options, (options) =>
            {
                AgendaContext context = new (options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();

                return context;
            });

            _mapperMock = A.Fake<IMapper>(x => x.Wrapping(AutoMapperConfig.Build().CreateMapper()));
            _unitOfWorkFactory = A.Fake<IUnitOfWorkFactory>(x => x.Wrapping(_unitOfWorkFactory));

            _sut = new HandleCreateAppointmentInfoCommand(_unitOfWorkFactory, _mapperMock);
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Clear();
                uow.Repository<Appointment>().Clear();

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _unitOfWorkFactory = null;
            _mapperMock = null;

            _sut = null;
        }

        public static IEnumerable<object[]> ValidInfoCreateRecordCases
        {
            get
            {
                Person person = new();
                Faker<AttendeeInfo> participantFaker = new Faker<AttendeeInfo>()
                    .RuleFor(x => x.Id, () => Guid.NewGuid())
                    .RuleFor(x => x.Name, _ => new Person().FullName )
                    .RuleFor(x => x.UpdatedDate, faker => faker.Noda().Instant.Recent())
                    ;

                Faker<NewAppointmentInfo> newAppointmentFaker = new Faker<NewAppointmentInfo>()
                    .RuleFor(x => x.StartDate, faker => faker.Noda().ZonedDateTime.Future())
                    .RuleFor(x => x.EndDate, (_, app) => app.StartDate.Plus(30.Minutes().ToDuration()))
                    .RuleFor(x => x.Location, faker => faker.Address.City())
                    .RuleFor(x => x.Subject, faker => faker.Lorem.Sentence())
                    .RuleFor(x => x.Attendees, faker => participantFaker.Generate(faker.Random.Int(min: 1, max: 5)));
                {
                    NewAppointmentInfo data = newAppointmentFaker.Generate();
                    yield return new object[]
                    {
                        data,
                        (Expression<Func<AppointmentInfo, bool>>)(app => app.Id != default
                                                                         && app.StartDate == data.StartDate.ToInstant()
                                                                         && app.EndDate == data.EndDate.ToInstant()
                                                                         && app.Subject == data.Subject
                                                                         && app.Location == data.Location
                                                                         && app.Attendees.Exactly(data.Attendees.Count())
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ValidInfoCreateRecordCases))]
        public async Task GivenValidInfo_Handler_CreatesResource(NewAppointmentInfo info, Expression<Func<AppointmentInfo, bool>> createdResourceExpectation)
        {
            _outputHelper.WriteLine($"{nameof(info)} : {info}");

            // Arrange
            CreateAppointmentInfoCommand cmd = new (info);

            // Act
            AppointmentInfo appointment = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            appointment.Should()
                .NotBeNull().And
                .Match(createdResourceExpectation);

            A.CallTo(() => _mapperMock.Map<AttendeeInfo, Attendee>(A<AttendeeInfo>.Ignored))
                .MustHaveHappened(info.Attendees.Count(), Times.Exactly);
            A.CallTo(() => _unitOfWorkFactory.NewUnitOfWork()).MustHaveHappenedOnceExactly();

            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                bool createdInDatastore = await uow.Repository<Appointment>().AnyAsync(x => x.Id == appointment.Id)
                    .ConfigureAwait(false);
                createdInDatastore.Should()
                    .BeTrue();
            }
        }
    }
}

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
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleCreateAppointmentInfoCommandTests : IDisposable, IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactoryMock;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IMapper _mapperMock;
        private HandleCreateAppointmentInfoCommand _sut;

        public HandleCreateAppointmentInfoCommandTests(ITestOutputHelper outputHelper, DatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<AgendaContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<AgendaContext>();
            dbContextOptionsBuilder.UseSqlite(database.Connection)
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(warnings => warnings.Throw());

            _unitOfWorkFactory = new EFUnitOfWorkFactory<AgendaContext>(dbContextOptionsBuilder.Options, (options) =>
            {
                AgendaContext context = new AgendaContext(options);
                context.Database.EnsureCreated();

                return context;

            });

            _mapperMock = A.Fake<IMapper>(x => x.Strict());
            _unitOfWorkFactoryMock = A.Fake<IUnitOfWorkFactory>(x => x.Strict());

            _sut = new HandleCreateAppointmentInfoCommand(_unitOfWorkFactoryMock, _mapperMock);


        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _unitOfWorkFactoryMock.NewUnitOfWork())
            {
                uow.Repository<Participant>().Delete(x => true);
                uow.Repository<Appointment>().Delete(x => true);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _unitOfWorkFactoryMock = null;
            _mapperMock = null;

            _sut = null;
        }

        public static IEnumerable<object[]> ValidInfoCreateRecordCases
        {
            get
            {
                Faker<ParticipantInfo> participantFaker = new Faker<ParticipantInfo>()
                    .RuleFor(x => x.Id, () => Guid.NewGuid())
                    .RuleFor(x => x.Name, faker => faker.Name.FullName())
                    .RuleFor(x => x.UpdatedDate, faker => faker.Date.Recent())
                    ;

                Faker<NewAppointmentInfo> newAppointmentFaker = new Faker<NewAppointmentInfo>()
                    .RuleFor(x => x.StartDate, faker => faker.Date.Future())
                    .RuleFor(x => x.EndDate, (faker, app) => app.StartDate.Add(30.Minutes()))
                    .RuleFor(x => x.Location, faker => faker.Address.City())
                    .RuleFor(x => x.Subject, faker => faker.Lorem.Sentence())
                    .RuleFor(x => x.Participants, () => participantFaker.Generate(new Randomizer().Int(min : 1, max : 5)));
                {
                    NewAppointmentInfo data = newAppointmentFaker.Generate();
                    yield return new object[]
                    {
                        data,
                        ((Expression<Func<AppointmentInfo, bool>>)(app => app.Id != default 
                            && app.StartDate == data.StartDate 
                            && app.EndDate == data.EndDate
                            && app.Subject == data.Subject
                            && app.Location == data.Location
                            && app.Participants.Count() == data.Participants.Count()
                        ))
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
            IMapper mapper = AutoMapperConfig.Build().CreateMapper();

            A.CallTo(() => _unitOfWorkFactoryMock.NewUnitOfWork())
                .Returns(_unitOfWorkFactory.NewUnitOfWork());
            A.CallTo(() => _mapperMock.Map<NewAppointmentInfo, Appointment>(A<NewAppointmentInfo>.Ignored))
                .ReturnsLazily((NewAppointmentInfo data) => mapper.Map<NewAppointmentInfo, Appointment>(data));

            A.CallTo(() => _mapperMock.Map<Appointment, AppointmentInfo>(A<Appointment>.Ignored))
                .ReturnsLazily((Appointment data) => mapper.Map<Appointment, AppointmentInfo>(data));

            A.CallTo(() => _mapperMock.Map<ParticipantInfo, Participant>(A<ParticipantInfo>._))
                .ReturnsLazily((ParticipantInfo data) => mapper.Map<ParticipantInfo, Participant>(data));

            CreateAppointmentInfoCommand cmd = new CreateAppointmentInfoCommand(info);

            // Act
            AppointmentInfo appointment = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            appointment.Should()
                .NotBeNull().And
                .Match(createdResourceExpectation);

            A.CallTo(() => _mapperMock.Map<ParticipantInfo, Participant>(A<ParticipantInfo>.Ignored))
                .MustHaveHappened(info.Participants.Count(), Times.Exactly);
            A.CallTo(() => _unitOfWorkFactoryMock.NewUnitOfWork()).MustHaveHappenedOnceExactly();

            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                bool createdInDatastore = await uow.Repository<Appointment>().AnyAsync(x => x.UUID == appointment.Id)
                    .ConfigureAwait(false);
                createdInDatastore.Should()
                    .BeTrue();
            }
        }
    }
}

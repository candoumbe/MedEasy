using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.Mapping;
using Agenda.Objects;
using AutoMapper;
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.Abstractions;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleGetPageOfAppointmentInfoQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private HandleGetPageOfAppointmentInfoQuery _sut;
        private readonly ITestOutputHelper _outputHelper;

        public HandleGetPageOfAppointmentInfoQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            DbContextOptionsBuilder<AgendaContext> optionsBuilder = new DbContextOptionsBuilder<AgendaContext>();
            optionsBuilder.UseSqlite(database.Connection)
                .EnableSensitiveDataLogging();

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(optionsBuilder.Options, (options) =>
            {
                AgendaContext context = new AgendaContext(options);
                context.Database.EnsureCreated();
                return context;
            });
            _dateTimeServiceMock = new Mock<IDateTimeService>(Strict);
            _sut = new HandleGetPageOfAppointmentInfoQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder, _dateTimeServiceMock.Object);
            _outputHelper = outputHelper;
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Delete(x => true);
                uow.Repository<Appointment>().Delete(x => true);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            
            _uowFactory = null;
            _dateTimeServiceMock = null;
            _sut = null;
        }


        public static IEnumerable<object[]> HandleCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Appointment>(),
                    1.January(2010),
                    (1, 10),
                    ((Expression<Func<Page<AppointmentInfo>, bool>>)(page => page.Count == 1
                        && page.Total == 0
                        && page.Entries != null && page.Entries.Count() == 0
                    )),
                    "DataStore is empty"
                };

                yield return new object[]
                {
                    Enumerable.Empty<Appointment>(),
                    1.January(2010),
                    (2, 10),
                    ((Expression<Func<Page<AppointmentInfo>, bool>>)(page => page.Count == 1
                        && page.Total == 0
                        && page.Entries != null && page.Entries.Count() == 0
                    )),
                    "DataStore is empty"
                };
                {

                    Faker<Appointment> appointmentFaker = new Faker<Appointment>()
                        .RuleFor(x => x.Id, () => 0)
                        .RuleFor(x => x.StartDate, 10.April(2000))
                        .RuleFor(x => x.EndDate, (faker, app) => app.StartDate.Add(10.Hours()))
                        .RuleFor(x => x.Location, faker=> faker.Address.City())
                        .RuleFor(x => x.Subject, faker=> faker.Lorem.Sentence(wordCount : 5))
                        ;

                    IEnumerable<Appointment> items = appointmentFaker.Generate(50);
                    yield return new object[]
                    {
                        items,
                        10.April(2000),
                        (2, 10),
                        ((Expression<Func<Page<AppointmentInfo>, bool>>)(page => page.Count == 5
                            && page.Total == 50
                            && page.Entries != null && page.Entries.Count() == 10
                        )),
                        "DataStore contains elements"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(HandleCases))]
        public async Task TestHandle(IEnumerable<Appointment> appointments, DateTimeOffset currentDateTime, (int page, int pageSize) pagination, Expression<Func<Page<AppointmentInfo>, bool>> pageExpectation, string reason)
        {
            // Arrange
            _outputHelper.WriteLine($"page : {pagination.page}");
            _outputHelper.WriteLine($"pageSize : {pagination.pageSize}");
            _outputHelper.WriteLine($"Current date time : {currentDateTime}");

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointments);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                int appointmentsCount = await uow.Repository<Appointment>().CountAsync()
                    .ConfigureAwait(false);
                _outputHelper.WriteLine($"DataStore count : {appointmentsCount}");
            }
            _dateTimeServiceMock.Setup(mock => mock.UtcNowOffset()).Returns(currentDateTime);
            GetPageOfAppointmentInfoQuery request = new GetPageOfAppointmentInfoQuery(new PaginationConfiguration { Page = pagination.page, PageSize = pagination.pageSize });

            // Act
            Page<AppointmentInfo> page = await _sut.Handle(request, ct: default)
                .ConfigureAwait(false);

            // Assert
            page.Should()
                .Match(pageExpectation, reason);
        }


    }
}

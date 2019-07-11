using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Agenda.Mapping;
using Agenda.Objects;
using AutoMapper.QueryableExtensions;
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
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
    [Feature("Search")]
    public class HandleSearchAppointmentInfoQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IHandleSearchQuery _searchQueryHandler;
        private HandleSearchAppointmentInfoQuery _sut;
        private IUnitOfWorkFactory _uowFactory;
        private IExpressionBuilder _expressionBuilder;

        public HandleSearchAppointmentInfoQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
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

            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;
            _outputHelper = outputHelper;
            _searchQueryHandler = new HandleSearchQuery(_uowFactory, _expressionBuilder);
            _sut = new HandleSearchAppointmentInfoQuery(_searchQueryHandler);
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Clear();
                uow.Repository<Appointment>().Clear();

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _uowFactory = null;
            _expressionBuilder = null;
            _searchQueryHandler = null;
            _sut = null;
        }

        public static IEnumerable<object[]> HandleCases
        {
            get
            {
                {
                    SearchAppointmentInfo searchAppointmentInfo = new SearchAppointmentInfo
                    {
                        From = 1.January(2010),
                        To = 2.January(2010),
                        Page = 1,
                        PageSize = 10
                    };
                    yield return new object[]
                    {
                        Enumerable.Empty<Appointment>(),
                        searchAppointmentInfo,
                        (
                            expectedPageCount : 1,
                            expectedPageSize : searchAppointmentInfo.PageSize,
                            expetedTotal : 0,
                            itemsExpectation : (Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null && !items.Any())
                        )
                    };
                }

                {
                    Faker<Appointment> appointmentFaker = new Faker<Appointment>()
                        .CustomInstantiator(faker => new Appointment(
                            id: Guid.NewGuid(),
                            subject: faker.Lorem.Sentence(),
                            location: faker.Address.City(),
                            startDate: 1.January(2010).At(13.Hours()),
                            endDate : 1.January(2010).At(14.Hours())));

                    IEnumerable<Appointment> appointments = appointmentFaker.Generate(10);
                    yield return new object[]
                    {
                        appointments,
                        new SearchAppointmentInfo
                        {
                            From = 2.January(2010),
                            To = 2.January(2010),
                            Page = 1,
                            PageSize = 10
                        },
                        (
                            expectedPageCount : 1,
                            expectedPageSize : 10,
                            expetedTotal : 0,
                            itemsExpectation : (Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null && !items.Any())
                        )
                    };
                }

                {
                    Faker<Appointment> appointmentFaker = new Faker<Appointment>()
                        .CustomInstantiator(faker => new Appointment(
                            id: Guid.NewGuid(),
                            subject: faker.Lorem.Sentence(),
                            location: faker.Address.City(),
                            startDate: 1.January(2010).At(13.Hours()),
                            endDate: 1.January(2010).At(14.Hours())));

                    IEnumerable<Appointment> appointments = appointmentFaker.Generate(10);
                    SearchAppointmentInfo searchAppointmentInfo = new SearchAppointmentInfo
                    {
                        From = 2.January(2010),
                        To = 2.January(2010),
                        Page = 1,
                        PageSize = 10
                    };
                    yield return new object[]
                    {
                        appointments,
                        searchAppointmentInfo,
                        (
                            expectedPageCount : 1,
                            expectedPageSize : searchAppointmentInfo.PageSize,
                            expetedTotal : 0,
                            itemsExpectation : (Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null
                                && !items.Any()
                            )
                        )
                    };
                }

                {
                    Faker<Appointment> appointmentFaker = new Faker<Appointment>()
                        .CustomInstantiator(faker => new Appointment(
                            id: Guid.NewGuid(),
                            subject: faker.Lorem.Sentence(),
                            location: faker.Address.City(),
                            startDate: 1.January(2010).At(13.Hours()),
                            endDate: 2.January(2010).At(14.Hours())));

                    IEnumerable<Appointment> appointments = appointmentFaker.Generate(7);
                    SearchAppointmentInfo searchAppointmentInfo = new SearchAppointmentInfo
                    {
                        From = 2.January(2010),
                        To = 2.January(2010),
                        Page = 1,
                        PageSize = 5
                    };
                    yield return new object[]
                    {
                        appointments,
                        searchAppointmentInfo,
                        (
                            expectedPageCount : 2,
                            expectedPageSize : searchAppointmentInfo.PageSize,
                            expetedTotal : 7,
                            itemsExpectation : (Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null
                                && items.Count() == 5
                                && items.Count(x => x.StartDate >= searchAppointmentInfo.From || x.EndDate >= searchAppointmentInfo.From) == items.Count()
                            )
                        )
                    };
                }

                {
                    Faker<Appointment> appointmentFaker = new Faker<Appointment>()
                        .CustomInstantiator(faker => new Appointment(
                            id: Guid.NewGuid(),
                            subject: faker.Lorem.Sentence(),
                            location: faker.Address.City(),
                            startDate: 1.January(2010).At(13.Hours()),
                            endDate: 2.January(2010).At(14.Hours())));

                    IEnumerable<Appointment> appointments = appointmentFaker.Generate(7);
                    SearchAppointmentInfo searchAppointmentInfo = new SearchAppointmentInfo
                    {
                        From = 2.January(2010),
                        To = 2.January(2010),
                        Page = 2,
                        PageSize = 5
                    };
                    yield return new object[]
                    {
                        appointments,
                        searchAppointmentInfo,
                        (
                            expectedPageCount : 2,
                            expectedPageSize : searchAppointmentInfo.PageSize,
                            expetedTotal : 7,
                            itemsExpectation : (Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null
                                && items.Count() == 2
                                && items.Count(x => x.StartDate >= searchAppointmentInfo.From || x.EndDate >= searchAppointmentInfo.From) == items.Count()
                            )
                        )
                    };
                }

                {
                    Faker<Appointment> appointmentFaker = new Faker<Appointment>()
                        .CustomInstantiator(faker => new Appointment(
                            id: Guid.NewGuid(),
                            subject: faker.Lorem.Sentence(),
                            location: faker.Address.City(),
                            startDate: 1.January(2010).At(13.Hours()),
                            endDate: 2.January(2010).At(14.Hours())));

                    IEnumerable<Appointment> appointments = appointmentFaker.Generate(7);
                    SearchAppointmentInfo searchAppointmentInfo = new SearchAppointmentInfo
                    {
                        From = 1.January(2010),
                        Page = 2,
                        PageSize = 5
                    };
                    yield return new object[]
                    {
                        appointments,
                        searchAppointmentInfo,
                        (
                            expectedPageCount : 2,
                            expectedPageSize : searchAppointmentInfo.PageSize,
                            expetedTotal : 7,
                            itemsExpectation : (Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null
                                && items.Count() == 2
                                && items.Count(x => x.StartDate >= searchAppointmentInfo.From || x.EndDate >= searchAppointmentInfo.From) == items.Count()
                            )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(HandleCases))]
        public async Task GivenDataStoreHasRecords_Handle_Returns_Data(IEnumerable<Appointment> appointments, SearchAppointmentInfo searchCriteria,
            (int expectedPageCount, int expectedPageSize, int expectedTotal, Expression<Func<IEnumerable<AppointmentInfo>, bool>> itemsExpectation) expectations)
        {
            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointments);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"Datastore : {appointments.Stringify()}");
                _outputHelper.WriteLine($"Search criteria : {searchCriteria.Stringify()}");
            }

            SearchAppointmentInfoQuery request = new SearchAppointmentInfoQuery(searchCriteria);

            // Act
            Page<AppointmentInfo> page = await _sut.Handle(request, default)
                .ConfigureAwait(false);

            // Assert

            page.Should()
                .NotBeNull();
            page.Count.Should()
                .Be(expectations.expectedPageCount);
            page.Total.Should()
                .Be(expectations.expectedTotal);
            page.Size.Should()
                .Be(expectations.expectedPageSize);
            page.Entries.Should()
                .Match(expectations.itemsExpectation);
        }
    }
}

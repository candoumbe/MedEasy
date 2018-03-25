﻿using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Agenda.Mapping;
using Agenda.Objects;
using AutoMapper.QueryableExtensions;
using FluentAssertions;
using FluentAssertions.Extensions;
using GenFu;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Newtonsoft.Json.JsonConvert;
using static Newtonsoft.Json.Formatting;
using static Newtonsoft.Json.NullValueHandling;
using Agenda.CQRS.Features.Appointments.Queries;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleSearchAppointmentInfoQueryTests : IDisposable, IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IHandleSearchQuery _searchQueryHandler;
        private HandleSearchAppointmentInfoQuery _sut;
        private IUnitOfWorkFactory _uowFactory;
        private IExpressionBuilder _expressionBuilder;

        public HandleSearchAppointmentInfoQueryTests(ITestOutputHelper outputHelper, DatabaseFixture database)
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
            A.Reset();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Delete(x => true);
                uow.Repository<Appointment>().Delete(x => true);

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
                yield return new object[]
                {
                    Enumerable.Empty<Appointment>(),
                    new SearchAppointmentInfo
                    {
                        From = 1.January(2010),
                        To = 2.January(2010),
                        Page = 1,
                        PageSize = 10
                    },
                    (
                        expectedPageCount : 1,
                        expectedPageSize : 10,
                        expetedTotal : 0,
                        itemsExpectation : ((Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null && !items.Any()))
                    )
                };
                {
                    A.Configure<Appointment>()
                        .Fill(x => x.Id, () => 0)
                        .Fill(x => x.Location).AsCity()
                        .Fill(x => x.Subject).AsLoremIpsumWords(numberOfWords: 5)
                        .Fill(x => x.UUID, () => Guid.NewGuid())
                        .Fill(x => x.StartDate, 1.January(2010).Add(13.Hours()))
                        .Fill(x => x.EndDate, app => 1.January(2010).Add(14.Hours()));

                    IEnumerable<Appointment> appointments = A.ListOf<Appointment>(10);
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
                            itemsExpectation : ((Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null && !items.Any()))
                        )
                    };
                }

                {

                    A.Configure<Appointment>()
                        .Fill(x => x.Id, () => 0)
                        .Fill(x => x.Location).AsCity()
                        .Fill(x => x.Subject).AsLoremIpsumWords(numberOfWords: 5)
                        .Fill(x => x.UUID, () => Guid.NewGuid())
                        .Fill(x => x.StartDate, 1.January(2010).Add(13.Hours()))
                        .Fill(x => x.EndDate, app => 2.January(2010).Add(14.Hours()));

                    IEnumerable<Appointment> appointments = A.ListOf<Appointment>(10);
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
                            expectedPageSize : 10,
                            expetedTotal : 10,
                            itemsExpectation : ((Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null 
                                && items.Count() == 10
                                && items.Count(x => x.StartDate >= searchAppointmentInfo.From || x.EndDate >= searchAppointmentInfo.From) == items.Count()
                            ))
                        )
                    };
                }

                {

                    A.Configure<Appointment>()
                        .Fill(x => x.Id, () => 0)
                        .Fill(x => x.Location).AsCity()
                        .Fill(x => x.Subject).AsLoremIpsumWords(numberOfWords: 5)
                        .Fill(x => x.UUID, () => Guid.NewGuid())
                        .Fill(x => x.StartDate, 1.January(2010).Add(13.Hours()))
                        .Fill(x => x.EndDate, app => 2.January(2010).Add(14.Hours()));

                    IEnumerable<Appointment> appointments = A.ListOf<Appointment>(7);
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
                            itemsExpectation : ((Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null
                                && items.Count() == 5
                                && items.Count(x => x.StartDate >= searchAppointmentInfo.From || x.EndDate >= searchAppointmentInfo.From) == items.Count()
                            ))
                        )
                    };
                }

                {

                    A.Configure<Appointment>()
                        .Fill(x => x.Id, () => 0)
                        .Fill(x => x.Location).AsCity()
                        .Fill(x => x.Subject).AsLoremIpsumWords(numberOfWords: 5)
                        .Fill(x => x.UUID, () => Guid.NewGuid())
                        .Fill(x => x.StartDate, 1.January(2010).Add(13.Hours()))
                        .Fill(x => x.EndDate, app => 2.January(2010).Add(14.Hours()));

                    IEnumerable<Appointment> appointments = A.ListOf<Appointment>(7);
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
                            itemsExpectation : ((Expression<Func<IEnumerable<AppointmentInfo>, bool>>)(items => items != null
                                && items.Count() == 2
                                && items.Count(x => x.StartDate >= searchAppointmentInfo.From || x.EndDate >= searchAppointmentInfo.From) == items.Count()
                            ))
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
            string ToString(object o) => SerializeObject(o, new JsonSerializerSettings { Formatting = Indented, NullValueHandling = Ignore });

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointments);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"DataStore : {ToString(appointments)}");
                _outputHelper.WriteLine($"Search criteria : {ToString(searchCriteria)}");

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

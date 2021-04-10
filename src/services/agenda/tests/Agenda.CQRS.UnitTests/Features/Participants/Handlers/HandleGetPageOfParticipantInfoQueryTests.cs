using Agenda.CQRS.Features.Participants.Handlers;
using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.Mapping;
using Agenda.Objects;

using Bogus;

using FluentAssertions;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;

using Microsoft.EntityFrameworkCore;

using NodaTime;
using NodaTime.Testing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Participants.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleGetPageOfParticipantInfoQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private IUnitOfWorkFactory _uowFactory;
        private HandleGetPageOfAttendeeInfoQuery _sut;
        private readonly ITestOutputHelper _outputHelper;

        public HandleGetPageOfParticipantInfoQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            DbContextOptionsBuilder<AgendaContext> optionsBuilder = new();
            optionsBuilder.UseInMemoryDatabase($"{Guid.NewGuid()}")
                .EnableSensitiveDataLogging();

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(optionsBuilder.Options, (options) =>
{
                AgendaContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _sut = new HandleGetPageOfAttendeeInfoQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
            _outputHelper = outputHelper;
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Clear();

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _uowFactory = null;
            _sut = null;
        }

        public static IEnumerable<object[]> HandleCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Attendee>(),
                    (1, 10),
                    (Expression<Func<Page<AttendeeInfo>, bool>>)(page => page.Count == 1
                        && page.Total == 0
                        && page.Entries != null && !page.Entries.Any()
                    ),
                    "DataStore is empty"
                };

                yield return new object[]
                {
                    Enumerable.Empty<Attendee>(),
                    (page:2, pageSize: 10),
                    (Expression<Func<Page<AttendeeInfo>, bool>>)(page => page.Count == 1
                        && page.Total == 0
                        && page.Entries != null && page.Entries.Count() == 0
                    ),
                    "DataStore is empty"
                };
                {
                    Faker<Attendee> attendeeFaker = new Faker<Attendee>()
                        .CustomInstantiator((faker) => new Attendee(Guid.NewGuid(), faker.Person.FullName));

                    IEnumerable<Attendee> items = attendeeFaker.Generate(50);
                    yield return new object[]
                    {
                        items,
                        (page: 2, pageSize: 10),
                        (Expression<Func<Page<AttendeeInfo>, bool>>)(page => page.Count == 5
                            && page.Total == 50
                            && page.Entries != null && page.Entries.Count() == 10
                        ),
                        "DataStore contains elements"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(HandleCases))]
        public async Task TestHandle(IEnumerable<Attendee> appointments, (int page, int pageSize) pagination, Expression<Func<Page<AttendeeInfo>, bool>> pageExpectation, string reason)
        {
            // Arrange
            _outputHelper.WriteLine($"page : {pagination.page}");
            _outputHelper.WriteLine($"pageSize : {pagination.pageSize}");

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Create(appointments);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                int appointmentsCount = await uow.Repository<Attendee>().CountAsync()
                    .ConfigureAwait(false);
                _outputHelper.WriteLine($"DataStore count : {appointmentsCount}");
            }
            
            GetPageOfAttendeeInfoQuery request = new(new PaginationConfiguration { Page = pagination.page, PageSize = pagination.pageSize });

            // Act
            Page<AttendeeInfo> page = await _sut.Handle(request, cancellationToken: default)
                .ConfigureAwait(false);

            // Assert
            page.Should()
                .Match(pageExpectation, reason);
        }
    }
}

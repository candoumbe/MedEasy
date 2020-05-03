using Agenda.CQRS.Features.Participants.Handlers;
using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Agenda.Mapping;
using Agenda.Objects;
using AutoMapper;
using FluentAssertions;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using AutoMapper.QueryableExtensions;
using static Moq.MockBehavior;
using DataFilters;

namespace Agenda.CQRS.UnitTests.Features.Participants.Handlers
{
    [UnitTest]
    [Feature("Agenda")]
    public class HandleSearchParticipantInfoQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private Mock<IHandleSearchQuery> _handleSearchQueryMock;
        private HandleSearchAttendeeInfoQuery _sut;
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;

        public HandleSearchParticipantInfoQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<AgendaContext> optionsBuilder = new DbContextOptionsBuilder<AgendaContext>();
            optionsBuilder.UseSqlite(database.Connection);

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(optionsBuilder.Options, (options) =>
            {
                AgendaContext context = new AgendaContext(options);
                context.Database.EnsureCreated();
                return context;
            });
            _mapper = AutoMapperConfig.Build().CreateMapper();
            _handleSearchQueryMock = new Mock<IHandleSearchQuery>(Strict);

            _sut = new HandleSearchAttendeeInfoQuery(handleSearch : _handleSearchQueryMock.Object);
        }

        public void Dispose()
        {
            _handleSearchQueryMock = null;
            _mapper = null;
            _uowFactory = null;
            _sut = null;
        }

        [Fact]
        public void IsHandler()
        {
            Type handlerType = typeof(HandleSearchAttendeeInfoQuery);
            handlerType.Should()
                .NotBeAbstract().And
                .Implement<IRequestHandler<SearchAttendeeInfoQuery, Page<AttendeeInfo>>>();
        }

        public static IEnumerable<object[]> SearchCases
        {
            get
            {
                {
                    SearchAttendeeInfo searchAttendeeInfo = new SearchAttendeeInfo
                    {
                        Page = 1,
                        Name = "*Bat*",
                        Sort = "+name"
                    };
                    yield return new object[]
                    {
                        Enumerable.Empty<Attendee>(),
                        searchAttendeeInfo,
                        (Expression<Func<Page<AttendeeInfo>, bool>>)(page => !page.Entries.Any()
                            && page.Count == 1
                            && page.Total == 0
                            && page.Size == searchAttendeeInfo.PageSize)
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(IEnumerable<Attendee> participants, SearchAttendeeInfo data, Expression<Func<Page<AttendeeInfo>, bool>> expectation)
        {
            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Create(participants);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"{nameof(participants)} : {participants.Jsonify()}");
                _outputHelper.WriteLine($"Search : {data.Jsonify()}");
            }

            _handleSearchQueryMock.Setup(mock => mock.Search<Attendee, AttendeeInfo>(It.IsAny<SearchQuery<AttendeeInfo>>(), It.IsAny<CancellationToken>()))
                .Returns(async (SearchQuery<AttendeeInfo> request, CancellationToken ct) =>
                {
                    Expression<Func<Attendee, AttendeeInfo>> selector = AutoMapperConfig.Build().CreateMapper()
                        .ConfigurationProvider.ExpressionBuilder.GetMapExpression<Attendee, AttendeeInfo>();

                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        return await uow.Repository<Attendee>()
                            .WhereAsync(
                                selector,
                                predicate : request.Data.Filter?.ToExpression<AttendeeInfo>(),
                                request.Data.Sort,
                                pageSize: request.Data.PageSize,
                                page : request.Data.Page,
                                ct)
                            .ConfigureAwait(false);
                    }
                });

            SearchAttendeeInfoQuery query = new SearchAttendeeInfoQuery(data);

            // Act
            Page<AttendeeInfo> page = await _sut.Handle(query, default)
                .ConfigureAwait(false);

            // Assert
            _handleSearchQueryMock.Verify(mock => mock.Search<Attendee, AttendeeInfo>(It.IsAny<SearchQuery<AttendeeInfo>>(), It.IsAny<CancellationToken>()), Times.Once);
            _handleSearchQueryMock.VerifyNoOtherCalls();

            page.Should()
                .Match(expectation);
        }
    }
}

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
        private HandleSearchParticipantInfoQuery _sut;
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

            _sut = new HandleSearchParticipantInfoQuery(handleSearch : _handleSearchQueryMock.Object);
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
            Type handlerType = typeof(HandleSearchParticipantInfoQuery);
            handlerType.Should()
                .NotBeAbstract().And
                .Implement<IRequestHandler<SearchParticipantInfoQuery, Page<ParticipantInfo>>>();
        }

        public static IEnumerable<object[]> SearchCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Participant>(),
                    new SearchParticipantInfo
                    {
                        Page = 1,
                        Name = "*Bat*",
                        Sort= "+name"
                    },
                    (Expression<Func<Page<ParticipantInfo>, bool>>)(page => page == Page<ParticipantInfo>.Empty)
                };
            }
        }

        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(IEnumerable<Participant> participants, SearchParticipantInfo data, Expression<Func<Page<ParticipantInfo>, bool>> expectation)
        {
            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Create(participants);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"{nameof(participants)} : {participants.Stringify()}");
                _outputHelper.WriteLine($"Search : {data.Stringify()}");
            }

            _handleSearchQueryMock.Setup(mock => mock.Search<Participant, ParticipantInfo>(It.IsAny<SearchQuery<ParticipantInfo>>(), It.IsAny<CancellationToken>()))
                .Returns(async (SearchQuery<ParticipantInfo> request, CancellationToken ct) =>
                {
                    Expression<Func<Participant, ParticipantInfo>> selector = AutoMapperConfig.Build().CreateMapper()
                        .ConfigurationProvider.ExpressionBuilder.GetMapExpression<Participant, ParticipantInfo>();

                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        return await uow.Repository<Participant>()
                            .WhereAsync(
                                selector,
                                predicate : request.Data.Filter?.ToExpression<ParticipantInfo>(),
                                request.Data.Sort.ToOrderClause(),
                                pageSize: request.Data.PageSize,
                                page : request.Data.Page,
                                ct)
                            .ConfigureAwait(false);
                    }
                });

            SearchParticipantInfoQuery query = new SearchParticipantInfoQuery(data);

            // Act
            Page<ParticipantInfo> page = await _sut.Handle(query, default)
                .ConfigureAwait(false);

            // Assert
            _handleSearchQueryMock.Verify(mock => mock.Search<Participant, ParticipantInfo>(It.IsAny<SearchQuery<ParticipantInfo>>(), It.IsAny<CancellationToken>()), Times.Once);
            _handleSearchQueryMock.VerifyNoOtherCalls();

            page.Should()
                .Match(expectation);
        }
    }
}

namespace Measures.CQRS.UnitTests.Handlers.Patients
{

    using AutoMapper.QueryableExtensions;
    using FluentAssertions;
    using Measures.DataStores;
    using Measures.CQRS.Handlers.Subjects;
    using Measures.DTO;
    using Measures.Mapping;
    using Measures.Objects;
    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.DTO.Search;
    using MedEasy.IntegrationTests.Core;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using static DataFilters.FilterOperator;
    using static Moq.MockBehavior;
    using DataFilters;
    using NodaTime.Testing;
    using NodaTime;
    using Measures.Ids;
    using NodaTime.Testing.Extensions;

    [UnitTest]
    public class HandleSearchPatientInfoQueryTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly Mock<IHandleSearchQuery> _iHandleSearchQueryMock;
        private readonly HandleSearchSubjectInfosQuery _sut;
        private readonly Mock<IExpressionBuilder> _expressionBuilderMock;

        public HandleSearchPatientInfoQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<MeasuresStore> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<MeasuresStore>(database.OptionsBuilder.Options, (options) => {
                MeasuresStore store = new(options, new FakeClock(new Instant()));

                store.Database.EnsureCreated();
                return store;

                });

            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);

            _iHandleSearchQueryMock = new Mock<IHandleSearchQuery>(Strict);
            _sut = new HandleSearchSubjectInfosQuery(_iHandleSearchQueryMock.Object);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;

        public static IEnumerable<object[]> SearchPatientCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Subject>(),
                    new SearchQueryInfo<SubjectInfo>
                    {
                        Filter = new Filter(field : nameof(SubjectInfo.Name), @operator : EqualTo, value : "Bruce"),
                        Page = 1,
                        PageSize = 3
                    },
                    (Expression<Func<Page<SubjectInfo>, bool>>)(x => x != null
                        && !x.Entries.Any()
                        && x.Count == 1
                        && x.Size == 3)
                };

                {
                    SubjectId patientId = SubjectId.New();
                    yield return new object[]
                    {
                        new []
                        {
                            new Subject(SubjectId.New(), "bruce wayne", 13.July(1940)),
                            new Subject(SubjectId.New(), "dick grayson", 10.October(1960)),
                            new Subject(patientId, "damian wayne", 11.December(2000)),
                        },
                        new SearchQueryInfo<SubjectInfo>
                        {
                            Filter = new Filter(field : nameof(SubjectInfo.Name), @operator : Contains, value : "y"),
                            Page = 3,
                            PageSize = 1,
                            Sort = new Sort<SubjectInfo>(nameof(SubjectInfo.BirthDate))
                        },
                        (Expression<Func<Page<SubjectInfo>, bool>>)(x => x != null
                                                                        && x.Entries.Count() == 1
                                                                        && x.Entries.ElementAt(0).Id == patientId
                                                                        && x.Count == 3
                                                                        && x.Size == 1)
                    };
                }
            }
        }

        [Fact]
        public void GivenNullParameter_Ctor_ThrowsException()
        {
            // Act
            Action action = () => new HandleSearchSubjectInfosQuery(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [MemberData(nameof(SearchPatientCases))]
        public async Task SearchPatientInfos(IEnumerable<Subject> patients, SearchQueryInfo<SubjectInfo> search, Expression<Func<Page<SubjectInfo>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"search : {search}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Subject>().Create(patients);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _expressionBuilderMock.Setup(mock => mock.GetMapExpression(It.IsAny<Type>(), It.IsAny<Type>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((Type sourceType, Type destinationType, IDictionary<string, object> parameters, MemberInfo[] membersToExpand) => AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression(sourceType, destinationType, parameters, membersToExpand));

            _iHandleSearchQueryMock.Setup(mock => mock.Search<Subject, SubjectInfo>(It.IsAny<SearchQuery<SubjectInfo>>(), It.IsAny<CancellationToken>()))
                .Returns(async (SearchQuery<SubjectInfo> query, CancellationToken ct) =>
                {
                    Expression<Func<SubjectInfo, bool>> filter = query.Data.Filter.ToExpression<SubjectInfo>();
                    Expression<Func<Subject, SubjectInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder
                        .GetMapExpression<Subject, SubjectInfo>();

                    ISort<SubjectInfo> sort = query.Data.Sort ?? new Sort<SubjectInfo>(nameof(SubjectInfo.BirthDate), SortDirection.Descending);

                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

                    return await uow.Repository<Subject>()
                                    .WhereAsync(selector,
                                                filter,
                                                sort,
                                                query.Data.PageSize,
                                                query.Data.Page,
                                                ct)
                                    .ConfigureAwait(false);
                });

            // Act
            SearchQuery<SubjectInfo> searchQuery = new(search);
            Page<SubjectInfo> pageOfResult = await _sut.Handle(searchQuery, default)
                                                       .ConfigureAwait(false);

            // Assert
            _iHandleSearchQueryMock.Verify(mock => mock.Search<Subject, SubjectInfo>(It.IsAny<SearchQuery<SubjectInfo>>(), It.IsAny<CancellationToken>()), Times.Once);
            pageOfResult.Should()
                .Match(resultExpectation);
        }
    }
}

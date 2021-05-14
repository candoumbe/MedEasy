
using AutoMapper.QueryableExtensions;
using FluentAssertions;
using Measures.DataStores;
using Measures.CQRS.Handlers.Patients;
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

namespace Measures.CQRS.UnitTests.Handlers.Patients
{
    [UnitTest]
    public class HandleSearchPatientInfoQueryTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly Mock<IHandleSearchQuery> _iHandleSearchQueryMock;
        private readonly HandleSearchPatientInfosQuery _sut;
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
            _sut = new HandleSearchPatientInfosQuery(_iHandleSearchQueryMock.Object);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;

        public static IEnumerable<object[]> SearchPatientCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Patient>(),
                    new SearchQueryInfo<PatientInfo>
                    {
                        Filter = new Filter(field : nameof(PatientInfo.Name), @operator : EqualTo, value : "Bruce"),
                        Page = 1,
                        PageSize = 3
                    },
                    (Expression<Func<Page<PatientInfo>, bool>>)(x => x != null
                        && !x.Entries.Any()
                        && x.Count == 1
                        && x.Size == 3)
                };

                {
                    PatientId patientId = PatientId.New();
                    yield return new object[]
                    {
                        new []
                        {
                            new Patient(PatientId.New(), "bruce wayne"),
                            new Patient(PatientId.New(), "dick grayson"),
                            new Patient(patientId, "damian wayne"),
                        },
                        new SearchQueryInfo<PatientInfo>
                        {
                            Filter = new Filter(field : nameof(PatientInfo.Name), @operator : Contains, value : "y"),
                            Page = 3,
                            PageSize = 1
                        },
                        (Expression<Func<Page<PatientInfo>, bool>>)(x => x != null
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
            Action action = () => new HandleSearchPatientInfosQuery(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [MemberData(nameof(SearchPatientCases))]
        public async Task SearchPatientInfos(IEnumerable<Patient> patients, SearchQueryInfo<PatientInfo> search, Expression<Func<Page<PatientInfo>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"search : {search}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(patients);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _expressionBuilderMock.Setup(mock => mock.GetMapExpression(It.IsAny<Type>(), It.IsAny<Type>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((Type sourceType, Type destinationType, IDictionary<string, object> parameters, MemberInfo[] membersToExpand) => AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression(sourceType, destinationType, parameters, membersToExpand));

            _iHandleSearchQueryMock.Setup(mock => mock.Search<Patient, PatientInfo>(It.IsAny<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()))
                .Returns(async (SearchQuery<PatientInfo> query, CancellationToken ct) =>
                {
                    Expression<Func<PatientInfo, bool>> filter = query.Data.Filter.ToExpression<PatientInfo>();
                    Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder
                        .GetMapExpression<Patient, PatientInfo>();

                    ISort<PatientInfo> sort = query.Data.Sort ?? new Sort<PatientInfo>(nameof(PatientInfo.BirthDate), SortDirection.Descending);

                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

                    return await uow.Repository<Patient>()
                                    .WhereAsync(selector,
                                                filter,
                                                sort,
                                                query.Data.PageSize,
                                                query.Data.Page,
                                                ct)
                                    .ConfigureAwait(false);
                });

            // Act
            SearchQuery<PatientInfo> searchQuery = new(search);
            Page<PatientInfo> pageOfResult = await _sut.Handle(searchQuery, default)
                                                       .ConfigureAwait(false);

            // Assert
            _iHandleSearchQueryMock.Verify(mock => mock.Search<Patient, PatientInfo>(It.IsAny<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()), Times.Once);
            pageOfResult.Should()
                .Match(resultExpectation);
        }
    }
}

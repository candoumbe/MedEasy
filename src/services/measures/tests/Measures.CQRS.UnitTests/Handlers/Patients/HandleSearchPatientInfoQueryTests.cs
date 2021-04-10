
using AutoMapper.QueryableExtensions;
using FluentAssertions;
using Measures.Context;
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
using Microsoft.EntityFrameworkCore;
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

namespace Measures.CQRS.UnitTests.Handlers.Patients
{
    [UnitTest]
    public class HandleSearchPatientInfoQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<IHandleSearchQuery> _iHandleSearchQueryMock;
        private HandleSearchPatientInfosQuery _sut;
        private Mock<IExpressionBuilder> _expressionBuilderMock;

        public HandleSearchPatientInfoQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
            //builder.UseSqlite(database.Connection);
            builder.UseInMemoryDatabase($"{Guid.NewGuid()}");

            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, (options) => {
                MeasuresContext context = new MeasuresContext(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);

            _iHandleSearchQueryMock = new Mock<IHandleSearchQuery>(Strict);
            _sut = new HandleSearchPatientInfosQuery(_iHandleSearchQueryMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _uowFactory = null;
            _expressionBuilderMock = null;

            _sut = null;
        }

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
                    ((Expression<Func<Page<PatientInfo>, bool>>)(x => x != null
                        && !x.Entries.Any()
                        && x.Count == 1
                        && x.Size == 3))
                };

                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new []
                        {
                            new Patient(Guid.NewGuid(), "bruce wayne"),
                            new Patient(Guid.NewGuid(), "dick grayson"),
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
                .Returns(async (SearchQuery<PatientInfo> query, CancellationToken ct) => {
                    {
                        Expression<Func<PatientInfo, bool>> filter = query.Data.Filter.ToExpression<PatientInfo>();
                        Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder
                            .GetMapExpression<Patient, PatientInfo>();

                        //ISort<PatientInfo> sort = (query.Data.Sort ?? );

                        using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

                        return await uow.Repository<Patient>()
                                        .WhereAsync(selector, filter, new Sort<PatientInfo>(nameof(PatientInfo.UpdatedDate), SortDirection.Descending), query.Data.PageSize, query.Data.Page, ct)
                                        .ConfigureAwait(false);
                    }
                });

            // Act
            SearchQuery<PatientInfo> searchQuery = new SearchQuery<PatientInfo>(search);
            Page<PatientInfo> pageOfResult = await _sut.Handle(searchQuery, default)
                .ConfigureAwait(false);

            // Assert
            _iHandleSearchQueryMock.Verify(mock => mock.Search<Patient, PatientInfo>(It.IsAny<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()), Times.Once);
            pageOfResult.Should()
                .Match(resultExpectation);
        }
    }
}

namespace Measures.CQRS.UnitTests.Handlers
{
    using AutoMapper.QueryableExtensions;
    using FluentAssertions;
    using Measures.DTO;
    using Measures.Mapping;
    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.DTO.Search;
    using Microsoft.Extensions.Logging;
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
    using static Moq.MockBehavior;
    using static DataFilters.FilterOperator;
    using DataFilters;
    using Measures.Ids;

    [UnitTest]
    public class HandleSearchQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _uowFactoryMock;
        private HandleSearchQuery _iHandleSearchQuery;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private readonly Mock<ILogger<HandleSearchQuery>> _loggerMock;

        public HandleSearchQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _uowFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _uowFactoryMock.Setup(mock => mock.NewUnitOfWork().Dispose());

            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);
            _loggerMock = new Mock<ILogger<HandleSearchQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));


            _iHandleSearchQuery = new HandleSearchQuery(_uowFactoryMock.Object, _expressionBuilderMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _uowFactoryMock = null;
            _expressionBuilderMock = null;

            _iHandleSearchQuery = null;
        }


        public static IEnumerable<object[]> SearchPatientCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Objects.Subject>(),
                    new SearchQueryInfo<SubjectInfo>
                    {
                        Filter = new Filter(field : nameof(SubjectInfo.Name), @operator : EqualTo, value : "Bruce"),
                        Page = 1,
                        PageSize = 3,
                        Sort = new Sort<SubjectInfo>(nameof(SubjectInfo.Name))
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
                            new Objects.Subject(SubjectId.New(), "bruce wayne"),
                            new Objects.Subject(SubjectId.New(), "dick grayson"),
                            new Objects.Subject(patientId, "damian wayne")
                        },
                        new SearchQueryInfo<SubjectInfo>
                        {
                            Filter = new Filter(field : nameof(SubjectInfo.Name), @operator : Contains, value : "y"),
                            Page = 3,
                            PageSize = 1,
                            Sort = new Sort<SubjectInfo>(nameof(SubjectInfo.Name))
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


        [Theory]
        [MemberData(nameof(SearchPatientCases))]
        public async Task SearchPatientInfos(IEnumerable<Objects.Subject> patients, SearchQueryInfo<SubjectInfo> search, Expression<Func<Page<SubjectInfo>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"search : {search}");

            // Arrange
            _expressionBuilderMock.Setup(mock => mock.GetMapExpression(It.IsAny<Type>(), It.IsAny<Type>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((Type sourceType, Type destinationType, IDictionary<string, object> parameters, MemberInfo[] membersToExpand) => AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression(sourceType, destinationType, parameters, membersToExpand));

            _uowFactoryMock.Setup(mock => mock.NewUnitOfWork().Repository<Objects.Subject>().WhereAsync(It.IsAny<Expression<Func<Objects.Subject, SubjectInfo>>>(),
                It.IsAny<Expression<Func<SubjectInfo, bool>>>(), It.IsAny<ISort<SubjectInfo>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns((Expression<Func<Objects.Subject, SubjectInfo>> selector, Expression<Func<SubjectInfo, bool>> filter, ISort<SubjectInfo> sorts, int pageSize, int page, CancellationToken cancellationToken)
                    =>
                    {
                        IEnumerable<SubjectInfo> results = patients.Select(selector.Compile())
                            .Where(filter.Compile())
                            .Skip(pageSize * (page - 1))
                            .Take(pageSize);

                        int total = patients.Select(selector.Compile())
                            .Count(filter.Compile());

                        return new ValueTask<Page<SubjectInfo>>(new Page<SubjectInfo>(results, total, pageSize));
                    });

            // Act
            SearchQuery<SubjectInfo> searchQuery = new(search);
            Page<SubjectInfo> pageOfResult = await _iHandleSearchQuery.Search<Objects.Subject, SubjectInfo>(searchQuery)
                .ConfigureAwait(false);

            // Assert
            pageOfResult.Should()
                .Match(resultExpectation);
        }
    }
}

using AutoMapper.QueryableExtensions;
using FluentAssertions;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO;
using MedEasy.DTO.Search;
using MedEasy.Mapping;
using MedEasy.Queries.Search;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.Data.DataFilterOperator;
using static Moq.MockBehavior;

namespace MedEasy.Handlers.Tests.Handlers
{
    public class HandleSearchQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _uowFactoryMock;
        private HandleSearchQuery _iHandleSearchQuery;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private Mock<ILogger<HandleSearchQuery>> _loggerMock;

        public HandleSearchQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _uowFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _uowFactoryMock.Setup(mock => mock.New().Dispose());

            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);
            _loggerMock = new Mock<ILogger<HandleSearchQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));


            _iHandleSearchQuery = new HandleSearchQuery(_uowFactoryMock.Object, _expressionBuilderMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _uowFactoryMock = null;
            _expressionBuilderMock = null;

            _iHandleSearchQuery = null;
        }


        public static IEnumerable<object> SearchPatientCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Objects.Patient>(),
                    new SearchQueryInfo<PatientInfo>
                    {
                        Filter = new DataFilter { Field = nameof(PatientInfo.Firstname), Operator = EqualTo, Value = "Bruce" },
                        Page = 1,
                        PageSize = 3
                    },
                    ((Expression<Func<IPagedResult<PatientInfo>, bool>>)(x => x != null && 
                        !x.Entries.Any() && 
                        x.PageCount == 0 && 
                        x.PageSize == 3))
                };

                yield return new object[]
               {
                    new []
                    {
                        new Objects.Patient {Id = 1, Firstname = "bruce", Lastname = "wayne" },
                        new Objects.Patient {Id = 2, Firstname = "dick", Lastname = "grayson" },
                        new Objects.Patient {Id = 3, Firstname = "damian", Lastname = "wayne" },

                    },
                    new SearchQueryInfo<PatientInfo>
                    {
                        Filter = new DataFilter { Field = nameof(PatientInfo.Lastname), Operator = Contains, Value = "y" },
                        Page = 3,
                        PageSize = 1
                    },
                    ((Expression<Func<IPagedResult<PatientInfo>, bool>>)(x => x != null && 
                        x.Entries.Count() == 1 && 
                        x.Entries.ElementAt(0).Id == 3 && 
                        x.PageCount == 3 && 
                        x.PageSize == 1))
               };
            }
        }


        [Theory]
        [MemberData(nameof(SearchPatientCases))]
        public async Task SearchPatientInfos(IEnumerable<Objects.Patient> patients, SearchQueryInfo<PatientInfo> search, Expression<Func<IPagedResult<PatientInfo>, bool>> resultExpectation )
        {
            _outputHelper.WriteLine($"search : {search}");

            // Arrange
            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<Objects.Patient, PatientInfo>(It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
                .Returns(AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder.CreateMapExpression<Objects.Patient, PatientInfo>());

            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().WhereAsync(It.IsAny<Expression<Func<Objects.Patient, PatientInfo>>>(),
                It.IsAny<Expression<Func<PatientInfo, bool>>>(), It.IsAny<IEnumerable<OrderClause<PatientInfo>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((Expression<Func<Objects.Patient, PatientInfo>> selector, Expression<Func<PatientInfo, bool>> filter, IEnumerable<OrderClause<PatientInfo>> sorts, int pageSize, int page)
                    => Task.Run(() =>
                    {
                        
                        IEnumerable<PatientInfo> results = patients.Select(selector.Compile())
                            .Where(filter.Compile())
                            .Skip(pageSize * (page - 1))
                            .Take(pageSize);

                        int total = patients.Select(selector.Compile())
                            .Count(filter.Compile());

                        return (IPagedResult<PatientInfo>)new PagedResult<PatientInfo>(results, total, pageSize);
                    }));

            // Act
            SearchQuery<PatientInfo> searchQuery = new SearchQuery<PatientInfo>(search);
            IPagedResult<PatientInfo> pageOfResult = await _iHandleSearchQuery.Search<Objects.Patient, PatientInfo>(searchQuery);

            // Assert
            pageOfResult.Should()
                .Match(resultExpectation);

            
        }



        
    }
}

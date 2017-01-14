using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Specialty.Queries;
using MedEasy.Queries.Specialty;
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System.Linq;
using AutoMapper.QueryableExtensions;
using MedEasy.Mapping;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Handlers.Tests.Specialty.Queries
{
    public class HandleFindDoctorsBySpecialtyIdQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleFindDoctorsBySpecialtyIdQuery _handler;

        private Mock<ILogger<HandleFindDoctorsBySpecialtyIdQuery>> _loggerMock;
        private Mock<IExpressionBuilder> _expressionBuilderMock;

        public HandleFindDoctorsBySpecialtyIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;


            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleFindDoctorsBySpecialtyIdQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);

            _handler = new HandleFindDoctorsBySpecialtyIdQuery(_unitOfWorkFactoryMock.Object, _loggerMock.Object, _expressionBuilderMock.Object);
        }


        public static IEnumerable<object> CtorCases
        {
            get
            {
                yield return new object[] { null, null, null };
                yield return new object[] { new Mock<IUnitOfWorkFactory>().Object, null, null };
                yield return new object[] { null, new Mock<ILogger<HandleFindDoctorsBySpecialtyIdQuery>>().Object, null };
                yield return new object[] { null, null, new Mock<IExpressionBuilder>().Object };
            }
        }


        public static IEnumerable<object> FindDoctorsBySpecialtyIdCases
        {
            get
            {
                yield return new object[]
                    {
                        new [] {
                            new Objects.Doctor { Id = 1, Firstname = "Henry", Lastname = "Jekyll", SpecialtyId = 2 }
                        },
                        1,
                        new PaginationConfiguration(),
                        1,
                        1,
                        0,
                        ((Expression<Func<IEnumerable<DoctorInfo>, bool>>) (items => items.Count() == 0))

                };

                yield return new object[]
                    {
                        new [] {
                            new Objects.Doctor {Id = 1, Firstname = "Henry", Lastname = "Jekyll", SpecialtyId = 1 }
                        },
                        1,
                        new PaginationConfiguration {Page = 1, PageSize = 1 },
                        1,
                        1,
                        1,
                        ((Expression<Func<IEnumerable<DoctorInfo>, bool>>) (items => items.Count() == 1 && items.Once(x => x.Id == 1)))

                };
            }
        }




        [Theory]
        [MemberData(nameof(CtorCases))]
        public void ShouldThrowArgumentNullException(IUnitOfWorkFactory factory, ILogger<HandleFindDoctorsBySpecialtyIdQuery> logger, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"ExpressionBuilder : {expressionBuilder}");
            Action action = () => new HandleFindDoctorsBySpecialtyIdQuery(factory, logger, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void HandlingNullQueryShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleFindDoctorsBySpecialtyIdQuery handler = new HandleFindDoctorsBySpecialtyIdQuery(
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<ILogger<HandleFindDoctorsBySpecialtyIdQuery>>(),
                    Mock.Of<IExpressionBuilder>());

                await handler.HandleAsync(null);
            };

            action.ShouldThrow<ArgumentNullException>("because the query to handle is null")
                .And.ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task QueryAnEmptyDatabase()
        {
            //Arrange

            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<Objects.Doctor, DoctorInfo>(It.IsAny<IDictionary<string, object>>()))
               .Returns(AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder.CreateMapExpression<Objects.Doctor, DoctorInfo>())
               .Verifiable();


            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Doctor>()
                .WhereAsync(It.IsAny<Expression<Func<Objects.Doctor, DoctorInfo>>>(), It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<IEnumerable<OrderClause<DoctorInfo>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((Expression<Func<Objects.Doctor, DoctorInfo>> selector, Expression<Func<Objects.Doctor, bool>> filter, IEnumerable<OrderClause<DoctorInfo>> order, int pageSize, int page)
                    => Task.FromResult<IPagedResult<DoctorInfo>>(new PagedResult<DoctorInfo>(Enumerable.Empty<DoctorInfo>(), 0, pageSize)))
                .Verifiable();

            // Act

            IPagedResult<DoctorInfo> output = await _handler.HandleAsync(new FindDoctorsBySpecialtyIdQuery(1, new PaginationConfiguration()));

            //Assert
            output.Should().NotBeNull();
            output.Entries.Should()
                .NotBeNull().And
                .BeEmpty();

            output.Total.Should().Be(0);
            output.PageCount.Should().Be(0);
            output.PageSize.Should().Be(PaginationConfiguration.DefaultPageSize);

            _expressionBuilderMock.VerifyAll();
            _unitOfWorkFactoryMock.VerifyAll();
        }


        [Theory]
        [MemberData(nameof(FindDoctorsBySpecialtyIdCases))]
        public async Task FindDoctorsBySpecialtyId(IEnumerable<Objects.Doctor> doctors, int specialtyId, PaginationConfiguration getQuery, int expectedPageSize,
            int expectedPage, int expectedTotal, Expression<Func<IEnumerable<DoctorInfo>, bool>> itemsExpectation)
        {

            _outputHelper.WriteLine($"{nameof(doctors)} : {SerializeObject(doctors)}");
            _outputHelper.WriteLine($"{nameof(specialtyId)} : {specialtyId}");


            // Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Doctor>()
                .WhereAsync(It.IsAny<Expression<Func<Objects.Doctor, DoctorInfo>>>(), It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<IEnumerable<OrderClause<DoctorInfo>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((Expression<Func<Objects.Doctor, DoctorInfo>> selector, Expression<Func<Objects.Doctor, bool>> filter, IEnumerable<OrderClause<DoctorInfo>> order, int pageSize, int page)
                    => Task.Factory.StartNew(() => {
                        IEnumerable<DoctorInfo> result = doctors.AsQueryable()
                            .Where(filter)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(selector);

                        int count = doctors.Count(filter.Compile());

                        IPagedResult<DoctorInfo> pagedResult = new PagedResult<DoctorInfo>(result, count, pageSize);

                        return pagedResult;
                    }))
                    .Verifiable();
            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<Objects.Doctor, DoctorInfo>(It.IsAny<IDictionary<string, object>>()))
                .Returns(AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder.CreateMapExpression<Objects.Doctor, DoctorInfo>())
                .Verifiable();
            
            // Act
            IPagedResult<DoctorInfo> pageOfResult = await _handler.HandleAsync(new FindDoctorsBySpecialtyIdQuery(specialtyId, getQuery));

            // Assert
            pageOfResult.Should().NotBeNull();
            pageOfResult.Entries.Should()
                .NotBeNull().And
                .Match(itemsExpectation);

            pageOfResult.Total.Should().Be(expectedTotal);

            _expressionBuilderMock.VerifyAll();
            _unitOfWorkFactoryMock.VerifyAll();


        }


        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactoryMock = null;
            _handler = null;
            _expressionBuilderMock = null;
        }
    }
}

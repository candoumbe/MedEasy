using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using AutoMapper;
using Xunit;
using FluentAssertions;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Mapping;
using MedEasy.Handlers.Specialty.Queries;
using MedEasy.RestObjects;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Specialty.Queries;
using MedEasy.API.Stores;
using Microsoft.EntityFrameworkCore;

namespace MedEasy.Handlers.Tests.Specialty.Queries
{
    public class HandleGetManySpecialtyInfosQueryTests: IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private HandleGetPageOfSpecialtyInfoQuery _handler;
        
        private Mock<ILogger<HandleGetPageOfSpecialtyInfoQuery>> _loggerMock;
        private IMapper _mapper;

        public HandleGetManySpecialtyInfosQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            DbContextOptionsBuilder<MedEasyContext> builder = new DbContextOptionsBuilder<MedEasyContext>();
            builder.UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(builder.Options);
            

            _loggerMock = new Mock<ILogger<HandleGetPageOfSpecialtyInfoQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _handler = new HandleGetPageOfSpecialtyInfoQuery(_unitOfWorkFactory, _loggerMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
        }


        public static IEnumerable<object[]> ConstructorCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    null,
                    null
                };

                yield return new object[]
                {
                    Mock.Of<ILogger<HandleGetSpecialtyInfoByIdQuery>>(),
                    null,
                    null
                };

                yield return new object[]
                {
                    null,
                    Mock.Of<IUnitOfWorkFactory>(),
                    null
                };

                yield return new object[]
                {
                    null,
                    null,
                    Mock.Of<IExpressionBuilder>()
                };

            }
        }


        

        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(ILogger<HandleGetSpecialtyInfoByIdQuery> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");

            Action action = () => new HandleGetSpecialtyInfoByIdQuery(factory, logger, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleGetSpecialtyInfoByIdQuery handler = new HandleGetSpecialtyInfoByIdQuery(
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<ILogger<HandleGetSpecialtyInfoByIdQuery>>(),
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
            // Arrange


            // Act
            IPagedResult<SpecialtyInfo> output = await _handler.HandleAsync(new GenericGetPageOfResourcesQuery<SpecialtyInfo>(new PaginationConfiguration()));

            //Assert
            output.Should().NotBeNull();
            output.Entries.Should().BeEmpty();
            output.Total.Should().Be(0);
            
        }
        



        public void Dispose()
        {
            _outputHelper = null;
           _unitOfWorkFactory = null;
            _handler = null;
            _mapper = null;
        }
    }
}

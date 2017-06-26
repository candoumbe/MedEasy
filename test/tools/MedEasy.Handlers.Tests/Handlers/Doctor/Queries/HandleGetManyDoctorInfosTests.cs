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
using MedEasy.Handlers.Doctor.Queries;
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Doctor.Queries;
using System.Threading;

namespace MedEasy.Handlers.Tests.Doctor.Queries
{
    public class HandleGetManyDoctorInfosQueryTests: IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetPageOfDoctorInfosQuery _handler;
        
        private Mock<ILogger<HandleGetPageOfDoctorInfosQuery>> _loggerMock;
        private IMapper _mapper;

        public HandleGetManyDoctorInfosQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetPageOfDoctorInfosQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _handler = new HandleGetPageOfDoctorInfosQuery(_unitOfWorkFactoryMock.Object, _loggerMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
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

            }
        }


        

        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(ILogger<HandleGetDoctorInfoByIdQuery> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            Action action = () => new HandleGetDoctorInfoByIdQuery(factory, logger, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleGetDoctorInfoByIdQuery handler = new HandleGetDoctorInfoByIdQuery(
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<ILogger<HandleGetDoctorInfoByIdQuery>>(),
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
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Doctor>()
                .ReadPageAsync(It.IsAny<Expression<Func<Objects.Doctor, DoctorInfo>>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<OrderClause<DoctorInfo>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<IPagedResult<DoctorInfo>>(PagedResult<DoctorInfo>.Default));

            // Act
            IPagedResult<DoctorInfo> output = await _handler.HandleAsync(new GenericGetPageOfResourcesQuery<DoctorInfo>(new PaginationConfiguration()));

            //Assert
            output.Should().NotBeNull();
            output.Entries.Should()
                .BeEmpty();

            output.Total.Should().Be(0);


            _unitOfWorkFactoryMock.Verify();
        }
        



        public void Dispose()
        {
            _outputHelper = null;
           _unitOfWorkFactoryMock = null;
            _handler = null;
            _mapper = null;
        }
    }
}

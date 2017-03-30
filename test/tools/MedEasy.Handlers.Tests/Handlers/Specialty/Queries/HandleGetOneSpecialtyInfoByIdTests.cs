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
using MedEasy.Queries.Specialty;
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.Handlers.Core.Specialty.Queries;

namespace MedEasy.Handlers.Tests.Specialty.Queries
{
    public class HandleGetOneSpecialtyInfoByIdQueryTests: IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetSpecialtyInfoByIdQuery _handler;
        
        private Mock<ILogger<HandleGetSpecialtyInfoByIdQuery>> _loggerMock;
        private IMapper _mapper;

        public HandleGetOneSpecialtyInfoByIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetSpecialtyInfoByIdQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _handler = new HandleGetSpecialtyInfoByIdQuery(_unitOfWorkFactoryMock.Object, _loggerMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
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
        public async Task UnknownIdShouldReturnNull()
        {
            //Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Specialty>()
                .SingleOrDefaultAsync(It.IsAny<Expression<Func<Objects.Specialty, SpecialtyInfo>>>(), It.IsAny<Expression<Func<Objects.Specialty, bool>>>()))
                .ReturnsAsync(null);

            // Act
            SpecialtyInfo output = await _handler.HandleAsync(new WantOneSpecialtyInfoByIdQuery(Guid.NewGuid()));

            //Assert
            output.Should().BeNull();
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

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentAssertions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Handlers.Core.Patient.Queries;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.Queries;
using MedEasy.Queries.Patient;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using Moq;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;

namespace MedEasy.Handlers.Tests.Patient.Queries
{
    public class HandleGetOneTemperatureInfoQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo> _handler;
        
        private Mock<ILogger<HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo>>> _loggerMock;
        private IMapper _mapper;
        private Mock<IValidate<IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo>>> _validatorMock;

        public HandleGetOneTemperatureInfoQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo>>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _validatorMock = new Mock<IValidate<IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo>>>(Strict);

            _handler = new HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo>( _unitOfWorkFactoryMock.Object, _loggerMock.Object,  _mapper.ConfigurationProvider.ExpressionBuilder);
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
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(ILogger<HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo>> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            Action action = () => new HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo>( factory, logger, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo> handler = new HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo>(
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<ILogger<HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo>>>(),
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
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Temperature>()
                .SingleOrDefaultAsync(It.IsAny<Expression<Func<Temperature, TemperatureInfo>>>(), It.IsAny<Expression<Func<Temperature, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Option<TemperatureInfo>>(Option.None<TemperatureInfo>()))
                .Verifiable();

            // Act
            Option<TemperatureInfo> output = await _handler.HandleAsync(new WantOnePhysiologicalMeasureQuery<TemperatureInfo>(Guid.NewGuid(), Guid.NewGuid()));

            //Assert
            output.Should()
                .Be(Option.None<TemperatureInfo>());

            _validatorMock.VerifyAll();
            _unitOfWorkFactoryMock.VerifyAll();
        }


        [Fact]
        public void ShouldThrowQueryNotValidExceptionIfValidationFails()
        {
            //Arrange
            
            // Act
            IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo> query = new WantOnePhysiologicalMeasureQuery<TemperatureInfo>(Guid.NewGuid(), Guid.NewGuid());
            Func<Task> action = async () =>  await _handler.HandleAsync(query);

            //Assert
            action.ShouldNotThrow<QueryNotValidException<Guid>>();


        }




        public void Dispose()
        {
            _outputHelper = null;
           _unitOfWorkFactoryMock = null;
            _handler = null;
            _mapper = null;
            _validatorMock = null;
        }
    }
}

﻿using System;
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
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Queries.Patient;
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using MedEasy.Objects;
using MedEasy.Handlers.Core.Patient.Queries;
using System.Threading;
using Optional;

namespace MedEasy.Handlers.Tests.Patient.Queries
{
    public class HandleGetMostRecentPhysiologicalMeasuresQueryTests: IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetMostRecentPhysiologicalMeasuresQuery<Temperature, TemperatureInfo> _handler;
        
        private Mock<ILogger<HandleGetMostRecentPhysiologicalMeasuresQuery<Temperature, TemperatureInfo>>> _loggerMock;
        private IMapper _mapper;

        public HandleGetMostRecentPhysiologicalMeasuresQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetMostRecentPhysiologicalMeasuresQuery<Temperature, TemperatureInfo>>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _handler = new HandleGetMostRecentPhysiologicalMeasuresQuery<Temperature, TemperatureInfo>(_unitOfWorkFactoryMock.Object, _loggerMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
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
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(ILogger<HandleGetMostRecentPhysiologicalMeasuresQuery<Temperature, TemperatureInfo>> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            
            Action action = () => new HandleGetMostRecentPhysiologicalMeasuresQuery<Temperature, TemperatureInfo>(factory,logger,  expressionBuilder);

            action.ShouldThrow<ArgumentNullException>("all parameters are mandatory").And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace("the parameter's name is mandatory to ease debugging");
        }


        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleGetMostRecentPhysiologicalMeasuresQuery<TemperatureInfo> handler = new HandleGetMostRecentPhysiologicalMeasuresQuery<Temperature, TemperatureInfo>(
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<ILogger<HandleGetMostRecentPhysiologicalMeasuresQuery<Temperature, TemperatureInfo>>>(),
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
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>()
                .AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(false));

            // Act
            Option<IEnumerable<TemperatureInfo>> output = await _handler.HandleAsync(new WantMostRecentPhysiologicalMeasuresQuery<TemperatureInfo>(new GetMostRecentPhysiologicalMeasuresInfo() { PatientId = Guid.NewGuid(), Count = 15 }));

            //Assert
            output.Should().Be(Option.None<IEnumerable<TemperatureInfo>>());

            _loggerMock.VerifyAll();
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
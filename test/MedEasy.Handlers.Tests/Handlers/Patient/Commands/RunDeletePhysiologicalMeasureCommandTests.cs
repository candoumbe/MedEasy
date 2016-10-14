using AutoMapper.QueryableExtensions;
using FluentAssertions;
using MedEasy.Commands.Patient;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Handlers.Exceptions;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.Validators;
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
using static Moq.MockBehavior;
using MedEasy.Commands;

namespace MedEasy.Handlers.Tests.Handlers.Patient.Commands
{
    public class RunDeleteOnePhysiologicalMeasureMeasureCommandTests : IDisposable
    {
        private Mock<ILogger<RunDeletePhysiologicalMeasureCommand<Guid, BloodPressure>>> _loggerMock;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private RunDeletePhysiologicalMeasureCommand<Guid, BloodPressure> _runner;
        private Mock<IValidate<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>> _validatorMock;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private ITestOutputHelper _outputHelper;

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
        public RunDeleteOnePhysiologicalMeasureMeasureCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<RunDeletePhysiologicalMeasureCommand<Guid, BloodPressure>>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();

            _validatorMock = new Mock<IValidate<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>>(Strict);
            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);

            _runner = new RunDeletePhysiologicalMeasureCommand<Guid, BloodPressure>(_validatorMock.Object, _loggerMock.Object,
                _unitOfWorkFactoryMock.Object);
        }


        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IValidate<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>> validator, ILogger<RunDeletePhysiologicalMeasureCommand<Guid, BloodPressure>> logger,
           IUnitOfWorkFactory factory)
        {
            Action action = () => new RunDeletePhysiologicalMeasureCommand<Guid, BloodPressure>(validator, logger, factory);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void HandlingNullCommandShouldThrowArgumentNullException()
        {
            Func<Task> action = () => _runner.RunAsync(null);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();

            
        }


        [Fact]
        public async Task ShouldDeleteTheResource()
        {
            // Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<BloodPressure>().Delete(It.IsAny<Expression<Func<BloodPressure, bool>>>()));
                
            _unitOfWorkFactoryMock.Setup(mock => mock.New().SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

       
            _validatorMock.Setup(mock => mock.Validate(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>())
                .Verifiable();

            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<DeletePhysiologicalMeasureInfo, BloodPressure>(It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((IDictionary<string, object> parameters, MemberInfo[] membersToExpand) =>
                    AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.CreateMapExpression<DeletePhysiologicalMeasureInfo, BloodPressure>(parameters, membersToExpand))
                .Verifiable();

            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<BloodPressure, BloodPressureInfo>(It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((IDictionary<string, object> parameters, MemberInfo[] membersToExpand) =>
                    AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.CreateMapExpression<BloodPressure, BloodPressureInfo>(parameters, membersToExpand))
                .Verifiable();

            // Act
            DeletePhysiologicalMeasureInfo input = new DeletePhysiologicalMeasureInfo
            {
                Id = 1,
                MeasureId = 12
            };
            await _runner.RunAsync(new DeleteOnePhysiologicalMeasureCommand<Guid>(Guid.NewGuid(), input));

            // Assert
            _validatorMock.Verify(mock => mock.Validate(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()), Times.Once);
            _unitOfWorkFactoryMock.VerifyAll();
            _loggerMock.VerifyAll();
        }

        

        public void Dispose()
        {
            _outputHelper = null;
        }
    }
}

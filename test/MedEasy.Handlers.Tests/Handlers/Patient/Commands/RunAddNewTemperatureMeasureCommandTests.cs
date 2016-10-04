using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using MedEasy.Commands.Patient;
using static MedEasy.Validators.ErrorLevel;
using static Moq.MockBehavior;
using Xunit;
using MedEasy.Handlers.Patient.Commands;
using FluentAssertions;
using MedEasy.Objects;
using System.Reflection;
using MedEasy.DTO;
using MedEasy.Mapping;
using System.Linq.Expressions;
using MedEasy.Handlers.Exceptions;
using MedEasy.Commands;

namespace MedEasy.Handlers.Tests.Handlers.Patient.Commands
{
    public class RunAddNewTemperatureMeasureCommandTests : IDisposable
    {
        private Mock<ILogger<RunAddNewTemperatureMeasureCommand>> _loggerMock;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private RunAddNewTemperatureMeasureCommand _runner;
        private Mock<IValidate<IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo>>> _validatorMock;
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
                    null,
                    null
                };

            }
        }
        public RunAddNewTemperatureMeasureCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<RunAddNewTemperatureMeasureCommand>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();

            _validatorMock = new Mock<IValidate<IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo>>>(Strict);
            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);

            _runner = new RunAddNewTemperatureMeasureCommand(_validatorMock.Object, _loggerMock.Object,
                _unitOfWorkFactoryMock.Object,
               _expressionBuilderMock.Object);
        }


        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IValidate<IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo>> validator, ILogger<RunAddNewTemperatureMeasureCommand> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            Action action = () => new RunAddNewTemperatureMeasureCommand(validator, logger, factory, expressionBuilder);

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
        public async Task ShouldCreateResource()
        {
            // Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Temperature>().Create(It.IsAny<Temperature>()))
                .Returns((Temperature temperature) => temperature);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>()))
               .ReturnsAsync(true)
               .Verifiable("because the runner should check that the patient exists before trying to create a measure on it");

            _validatorMock.Setup(mock => mock.Validate(It.IsAny<IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo>>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>())
                .Verifiable();

            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<CreateTemperatureInfo, Temperature>(It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((IDictionary<string, object> parameters, MemberInfo[] membersToExpand) =>
                    AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.CreateMapExpression<CreateTemperatureInfo, Temperature>(parameters, membersToExpand))
                .Verifiable();

            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<Temperature, TemperatureInfo>(It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((IDictionary<string, object> parameters, MemberInfo[] membersToExpand) =>
                    AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.CreateMapExpression<Temperature, TemperatureInfo>(parameters, membersToExpand))
                .Verifiable();

            // Act
            CreateTemperatureInfo input = new CreateTemperatureInfo
            {
                Id = 1,
                Value = 10,
                DateOfMeasure = DateTime.Now
                
            };
            TemperatureInfo output = await _runner.RunAsync(new AddNewPhysiologicalMeasureCommand<CreateTemperatureInfo, TemperatureInfo>(input));

            // Assert
            output.Should().NotBeNull();
            output.PatientId.Should().Be(input.Id);
            output.Value.Should().Be(input.Value);
            output.DateOfMeasure.Should().Be(input.DateOfMeasure);
            
            _validatorMock.Verify(mock => mock.Validate(It.IsAny<IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo>>()), Times.Once);
            _unitOfWorkFactoryMock.VerifyAll();
            _loggerMock.VerifyAll();
        }

        [Fact]
        public void ShouldThrowNotFoundExceptionIfUnknownPatient()
        {
            // Arrange
            
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>()))
               .ReturnsAsync(false)
               .Verifiable("because the runner should check that the patient exists before trying to create a measure on it");

            _validatorMock.Setup(mock => mock.Validate(It.IsAny<IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo>>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>())
                .Verifiable();

            

            // Act
            CreateTemperatureInfo input = new CreateTemperatureInfo
            {
                Id = 1,
                Value = 10,
                DateOfMeasure = DateTime.Now

            };
            var cmd = new AddNewPhysiologicalMeasureCommand<CreateTemperatureInfo, TemperatureInfo>(input);
            Func<Task> action = async() => await _runner.RunAsync(cmd);

            // Assert
            action.ShouldThrow<NotFoundException>();

            _validatorMock.Verify(mock => mock.Validate(It.IsAny<IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo>>()), Times.Once);
            _unitOfWorkFactoryMock.VerifyAll();
            _loggerMock.VerifyAll();
        }



        public void Dispose()
        {
            _outputHelper = null;
        }
    }
}

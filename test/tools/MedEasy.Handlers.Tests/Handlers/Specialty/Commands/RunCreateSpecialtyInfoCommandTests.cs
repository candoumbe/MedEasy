using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Specialty;
using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Specialty.Commands;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.Validators.ErrorLevel;
using static Moq.MockBehavior;
using FluentAssertions;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Exceptions;
using Optional;

namespace MedEasy.Handlers.Tests.Handlers
{
    public class RunCreateSpecialtyInfoCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IValidate<ICreateSpecialtyCommand>> _validatorMock;
        private Mock<ILogger<RunCreateSpecialtyCommand>> _loggerMock;
        private RunCreateSpecialtyCommand _runner;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private Mock<IUnitOfWorkFactory> _factoryMock;

        public RunCreateSpecialtyInfoCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _validatorMock = new Mock<IValidate<ICreateSpecialtyCommand>>(Strict);
            _loggerMock = new Mock<ILogger<RunCreateSpecialtyCommand>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);
            _factoryMock = new Mock<IUnitOfWorkFactory>(Strict);

            _runner = new RunCreateSpecialtyCommand(_validatorMock.Object, _loggerMock.Object, _factoryMock.Object, _expressionBuilderMock.Object);
        }




        [Fact]
        public async Task ShouldThrowCommandNotValidExceptionIfAnyValidationErrorFound()
        {
            // Arrange
            _validatorMock.Setup(mock => mock.Validate(It.IsAny<ICreateSpecialtyCommand>()))
                .Returns(new[]
                {
                    Task.FromResult(new ErrorInfo("ErrorCode", string.Empty, Error))
                });


            // Act
            ICreateSpecialtyCommand cmd = new CreateSpecialtyCommand(new CreateSpecialtyInfo { });
            Option<SpecialtyInfo, CommandException> result = await _runner.RunAsync(cmd);


            result.HasValue.Should().BeFalse();
            result.MatchNone(exception =>
            {
                exception.Should().BeOfType<CommandNotValidException<Guid>>().Which
                    .CommandId.Should().Be(cmd.Id);
                exception.Message.Should().NotBeNull();
                exception.Errors.Should()
                    .NotBeNullOrEmpty().And
                    .HaveCount(1).And
                    .ContainSingle(x => x.Severity == Error && x.Key == "ErrorCode");

                _loggerMock.Verify(mock => mock.Log(It.Is<LogLevel>(level => level == LogLevel.Debug), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeast(exception.Errors.Count()));
            });
            _loggerMock.Verify(mock => mock.Log(It.Is<LogLevel>(level => level == LogLevel.Information), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);


        }


        public void Dispose()
        {
            _outputHelper = null;
            _validatorMock = null;
            _loggerMock = null;
            _runner = null;
            _expressionBuilderMock = null;
            _factoryMock = null;
        }
    }
}

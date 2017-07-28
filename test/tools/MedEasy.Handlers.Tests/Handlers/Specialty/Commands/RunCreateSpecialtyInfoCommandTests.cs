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

        private RunCreateSpecialtyCommand _runner;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private Mock<IUnitOfWorkFactory> _factoryMock;

        public RunCreateSpecialtyInfoCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);
            _factoryMock = new Mock<IUnitOfWorkFactory>(Strict);

            _runner = new RunCreateSpecialtyCommand(_factoryMock.Object, _expressionBuilderMock.Object);
        }

        [Fact]
        public void Throws_ArgumentNullException_If_Command_Is_Null()
        {
            // Act
            Func<Task> action = async () => await _runner.RunAsync(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        public void Dispose()
        {
            _outputHelper = null;
            _runner = null;
            _expressionBuilderMock = null;
            _factoryMock = null;
        }
    }
}

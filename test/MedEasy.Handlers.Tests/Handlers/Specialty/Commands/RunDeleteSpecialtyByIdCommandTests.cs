using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using MedEasy.Validators;
using AutoMapper;
using Xunit;
using FluentAssertions;
using MedEasy.Commands.Specialty;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MedEasy.Handlers.Specialty.Commands;
using MedEasy.DAL.Interfaces;
using System.Linq;
using MedEasy.Mapping;
using System.Linq.Expressions;

namespace MedEasy.BLL.Tests.Handlers.Commands.Specialty
{
    public class RunDeleteSpecialtyByIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IValidate<IDeleteSpecialtyByIdCommand>> _validatorMock;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private RunDeleteSpecialtyByIdCommand _runner;
        
        private Mock<ILogger<RunDeleteSpecialtyByIdCommand>> _loggerMock;
        private IMapper _mapper;

        public RunDeleteSpecialtyByIdCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _validatorMock = new Mock<IValidate<IDeleteSpecialtyByIdCommand>>(Strict);
            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<RunDeleteSpecialtyByIdCommand>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _runner = new RunDeleteSpecialtyByIdCommand(_validatorMock.Object, _loggerMock.Object, _unitOfWorkFactoryMock.Object);
        }


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

        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IValidate<IDeleteSpecialtyByIdCommand> validator, ILogger<RunDeleteSpecialtyByIdCommand> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Validator : {validator}");
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            Action action = () => new RunDeleteSpecialtyByIdCommand(validator, logger, factory);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ValidCommandShouldDeleteTheResource()
        {
            //Arrange
            _validatorMock.Setup(mock => mock.Validate(It.IsAny<IDeleteSpecialtyByIdCommand>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>())
                .Verifiable();

            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Specialty>().Delete(It.IsAny<Expression<Func<Objects.Specialty, bool>>>()))
               .Verifiable();

            _unitOfWorkFactoryMock.Setup(mock => mock.New().SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();
            
            //Act
            DeleteSpecialtyByIdCommand command = new DeleteSpecialtyByIdCommand(1);
            _outputHelper.WriteLine($"Command : {command}");
            
            Func<Task> action = async () => await _runner.RunAsync(command);


            //Assert
            action.ShouldNotThrow<Exception>();

            _unitOfWorkFactoryMock.VerifyAll();
            _validatorMock.VerifyAll();

        }

        

        

        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                var handler = new RunDeleteSpecialtyByIdCommand(Validator<IDeleteSpecialtyByIdCommand>.Default,
                    Mock.Of<ILogger<RunDeleteSpecialtyByIdCommand>>(),
                    Mock.Of<IUnitOfWorkFactory>());

                await handler.RunAsync(null);
            };

            action.ShouldThrow<ArgumentNullException>();
        }

        
        
        public void Dispose()
        {
            _outputHelper = null;
            _validatorMock = null;
            _unitOfWorkFactoryMock = null;
            _runner = null;
            _mapper = null;
        }
    }
}

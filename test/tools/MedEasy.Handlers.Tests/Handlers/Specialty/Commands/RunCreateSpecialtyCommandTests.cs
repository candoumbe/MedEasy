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
using static MedEasy.Validators.ErrorLevel;
using System.Threading.Tasks;
using MedEasy.Handlers.Specialty.Commands;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using System.Linq;
using MedEasy.Mapping;
using System.Linq.Expressions;
using MedEasy.Handlers.Core.Specialty.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;

namespace MedEasy.BLL.Tests.Handlers.Commands.Specialty
{
    public class HandleCreateSpecialtyCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IValidate<ICreateSpecialtyCommand>> _validatorMock;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private RunCreateSpecialtyCommand _runner;
        
        private Mock<ILogger<RunCreateSpecialtyCommand>> _loggerMock;
        private IMapper _mapper;

        public HandleCreateSpecialtyCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _validatorMock = new Mock<IValidate<ICreateSpecialtyCommand>>(Strict);
            _mapper = AutoMapperConfig.Build().CreateMapper();
            DbContextOptionsBuilder<MedEasyContext> dbOptionsBuiler = new DbContextOptionsBuilder<MedEasyContext>();
            dbOptionsBuiler.UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(dbOptionsBuiler.Options);

            _loggerMock = new Mock<ILogger<RunCreateSpecialtyCommand>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _runner = new RunCreateSpecialtyCommand(_validatorMock.Object, _loggerMock.Object, _unitOfWorkFactory, _mapper.ConfigurationProvider.ExpressionBuilder);
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
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IValidate<ICreateSpecialtyCommand> validator, ILogger<RunCreateSpecialtyCommand> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Validator : {validator}");
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            Action action = () => new RunCreateSpecialtyCommand(validator, logger, factory, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ValidCommandShouldCreateTheResource()
        {
            //Arrange
            _validatorMock.Setup(mock => mock.Validate(It.IsAny<ICreateSpecialtyCommand>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>())
                .Verifiable();
            
            //Act
            CreateSpecialtyInfo input = new CreateSpecialtyInfo
            {
                Name = "médecine générale"
            };
            CreateSpecialtyCommand command = new CreateSpecialtyCommand(input);
            
            _outputHelper.WriteLine($"Command : {command}");
            SpecialtyInfo output = await _runner.RunAsync(command);


            //Assert
            output.Should().NotBeNull();
            output.Id.Should().NotBeEmpty();
            output.Name.Should().Be(input.Name.ToTitleCase());
            
            _validatorMock.VerifyAll();

        }

        /// <summary>
        /// Tests that running commands that would create a du
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void CreateAResourceWithANameThatIsNotAvailableShouldFail()
        {
            //Arrange
            _validatorMock.Setup(mock => mock.Validate(It.IsAny<ICreateSpecialtyCommand>()))
                .Returns((ICreateSpecialtyCommand cmd) => new Task<ErrorInfo>[] {
                    Task.FromResult(new ErrorInfo ("ErrDuplicate", $@"a resource with ""{nameof(SpecialtyInfo.Name)}"" : ""{cmd.Data.Name}"" already exists", Error ))
                });

            //Act
            ICreateSpecialtyCommand command = new CreateSpecialtyCommand(new CreateSpecialtyInfo { Name = "NameThatAlreadyExists" });
            Func<Task> action = async () => await _runner.RunAsync(command);

            CommandNotValidException<Guid> exception = action.ShouldThrow<CommandNotValidException<Guid>>()
                .Which;
            
            exception.CommandId.Should().Be(command.Id);
            exception.Errors.Should().ContainSingle();
            exception.Errors.Single().Key.Should().Be("ErrDuplicate");
            exception.Errors.Single().Description.Should().Match("*already exists");
            exception.Errors.Single().Severity.Should().Be(Error);

            _loggerMock.Verify(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeast(2));

        }


        /// <summary>
        /// Commands that should throws validation exceptions
        /// </summary>
        public static IEnumerable<object[]> HandlingInvalidCommands
        {
            get
            {
                Mock<IValidate<ICreateSpecialtyCommand>> validatorMock = new Mock<IValidate<ICreateSpecialtyCommand>>(Strict);
                validatorMock.Setup(mock => mock.Validate(It.IsAny<ICreateSpecialtyCommand>()))
                    .Returns((ICreateSpecialtyCommand command) => new[] {
                        Task.FromResult(new ErrorInfo(string.Empty, string.Empty, Error))
                    });

                yield return new object[]
                {
                    validatorMock.Object,
                    new CreateSpecialtyCommand(new CreateSpecialtyInfo())
                };
            }
        }

        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                var handler = new RunCreateSpecialtyCommand(Validator<ICreateSpecialtyCommand>.Default,
                    Mock.Of<ILogger<RunCreateSpecialtyCommand>>(),
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<IExpressionBuilder>());

                await handler.RunAsync(null);
            };

            action.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(HandlingInvalidCommands))]
        public void ShouldThrowCommandNotValidException(IValidate<ICreateSpecialtyCommand> validator, ICreateSpecialtyCommand command)
        {
            _outputHelper.WriteLine($"Command : {command}");
            _outputHelper.WriteLine($"Validator : {validator}");

            Func<Task> action = async () =>
            {
                Mock<ILogger<RunCreateSpecialtyCommand>> loggerMock = new Mock<ILogger<RunCreateSpecialtyCommand>>();
                Mock<IUnitOfWorkFactory> factoryMock = new Mock<IUnitOfWorkFactory>();
                Mock<IExpressionBuilder> expressionBuilderMock = new Mock<IExpressionBuilder>();
                IRunCreateSpecialtyCommand handler = new RunCreateSpecialtyCommand(validator,
                    loggerMock.Object,
                    factoryMock.Object,
                    expressionBuilderMock.Object);

                await handler.RunAsync(command);
            };

            action.ShouldThrow<CommandNotValidException<Guid>>()
                .And.Errors.Should().NotBeNullOrEmpty()
                .And.NotContainNulls();
        }

        [Fact]
        public async Task CreatingANewSpecialtyWithACodeAlreadyUsedByAnOtherShouldThrowCommandNotValidException()
        {

            //Arrange
            _validatorMock.Setup(mock => mock.Validate(It.IsAny<ICreateSpecialtyCommand>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>())
                .Verifiable("validation should already have occured");

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Objects.Specialty>().Create(new Objects.Specialty { Name = "médecine générale" });

                await uow.SaveChangesAsync();
            }

            //Act
            ICreateSpecialtyCommand command = new CreateSpecialtyCommand(new CreateSpecialtyInfo { Name = "Médecine générale" });
            Func<Task> action = async () => await _runner.RunAsync(command);

            //Assert
            CommandNotValidException<Guid> exceptionThrown = action.ShouldThrow<CommandNotValidException<Guid>>().Which;

            exceptionThrown.Errors.Should()
                .NotBeNullOrEmpty().And
                .ContainSingle().And
                .Contain(x => "ErrDuplicate".Equals(x.Key, StringComparison.OrdinalIgnoreCase) && x.Severity == Error);

            _validatorMock.Verify();


        }

        public void Dispose()
        {
            _outputHelper = null;
            _validatorMock = null;
            _unitOfWorkFactory = null;
            _runner = null;
            _mapper = null;
        }
    }
}

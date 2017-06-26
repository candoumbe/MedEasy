using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using Xunit;
using AutoMapper;
using FluentAssertions;
using MedEasy.Validators;
using MedEasy.Commands.Patient;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.DAL.Interfaces;
using MedEasy.Mapping;
using System.Threading.Tasks;
using MedEasy.DTO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Optional;
using MedEasy.Handlers.Core.Exceptions;

namespace MedEasy.BLL.Tests.Commands.Patient
{
    public class RunCreatePatientCommandTests : IDisposable
    {
        private Mock<ILogger<RunCreatePatientCommand>> _loggerMock;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private Mock<IMapper> _mapperMock;
        private RunCreatePatientCommand _handler;
        private Mock<IValidate<ICreatePatientCommand>> _validatorMock;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private ITestOutputHelper _outputHelper;


        public RunCreatePatientCommandTests(ITestOutputHelper output)
        {
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<RunCreatePatientCommand>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _validatorMock = new Mock<IValidate<ICreatePatientCommand>>(Strict);
            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);

            _outputHelper = output;
            _handler = new RunCreatePatientCommand(_validatorMock.Object, _loggerMock.Object,
                _unitOfWorkFactoryMock.Object,
               _expressionBuilderMock.Object);
        }

        public void Dispose()
        {
            _unitOfWorkFactoryMock = null;
            _loggerMock = null;
            _mapperMock = null;
            _outputHelper = null;
            _handler = null;

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

        public static IEnumerable<object> ValidCreatePatientInfoCases
        {
            get
            {
                yield return new object[] {
                    new CreatePatientInfo
                    {
                        Firstname = "Bruce",
                        Lastname = "Wayne",
                        MainDoctorId = Guid.NewGuid()
                    }
                };

                yield return new object[] {
                    new CreatePatientInfo
                    {
                        Firstname = "Cyrille-Alexandre",
                        Lastname = "NDOUMBE",
                        MainDoctorId = Guid.NewGuid()
                    }
                };

                yield return new object[] {
                    new CreatePatientInfo
                    {
                        Firstname = "cyrille-alexandre",
                        Lastname = "NDOUMBE",
                        MainDoctorId = Guid.NewGuid()
                    }
                };
            }
        }


        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IValidate<ICreatePatientCommand> validator, ILogger<RunCreatePatientCommand> logger,
            IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            Action action = () => new RunCreatePatientCommand(validator, logger, factory, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Theory]
        [MemberData(nameof(ValidCreatePatientInfoCases))]
        public async Task ShouldCreateResource(CreatePatientInfo input)
        {
            _outputHelper.WriteLine($"input : {input}");

            // Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().Create(It.IsAny<Objects.Patient>()))
                .Returns((Objects.Patient patient) => patient);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _validatorMock.Setup(mock => mock.Validate(It.IsAny<ICreatePatientCommand>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>());

            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<CreatePatientInfo, Objects.Patient>(It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((IDictionary<string, object> parameters, MemberInfo[] membersToExpand) =>
                    AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.CreateMapExpression<CreatePatientInfo, Objects.Patient>(parameters, membersToExpand));

            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<Objects.Patient, PatientInfo>(It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((IDictionary<string, object> parameters, MemberInfo[] membersToExpand) =>
                    AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.CreateMapExpression<Objects.Patient, PatientInfo>(parameters, membersToExpand));

            // Act

            Option<PatientInfo, CommandException> output = await _handler.RunAsync(new CreatePatientCommand(input));

            // Assert
            output.HasValue.Should().BeTrue();

            output.MatchSome(x =>
            {

                x.Should().NotBeNull();
                x.Firstname.Should().Be(input.Firstname?.ToTitleCase());
                x.Lastname.Should().Be(input.Lastname?.ToUpper());
                x.BirthDate.Should().Be(input.BirthDate);
                x.BirthPlace.Should().Be(input.BirthPlace?.ToTitleCase());
                x.MainDoctorId.ShouldBeEquivalentTo(input.MainDoctorId);
                _validatorMock.Verify(mock => mock.Validate(It.IsAny<ICreatePatientCommand>()), Times.Once);
            });

        }


    }

}

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

namespace MedEasy.Handlers.Tests.Commands.Patient
{
    public class RunCreatePatientCommandTests : IDisposable
    {
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private RunCreatePatientCommand _handler;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private ITestOutputHelper _outputHelper;


        public RunCreatePatientCommandTests(ITestOutputHelper output)
        {
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);

            _outputHelper = output;
            _handler = new RunCreatePatientCommand(_unitOfWorkFactoryMock.Object, _expressionBuilderMock.Object);
        }

        public void Dispose()
        {
            _unitOfWorkFactoryMock = null;
            _outputHelper = null;
            _handler = null;

        }


        public static IEnumerable<object[]> ConstructorCases
        {
            get
            {
                IEnumerable<object> factories = new[] { null, Mock.Of<IUnitOfWorkFactory>() };
                IEnumerable<object> expressionBuilders = new[] { null, Mock.Of<IExpressionBuilder>() };

                return factories.CrossJoin(expressionBuilders)
                    .Where(tuple => tuple.Item1 == null || tuple.Item2 == null)
                    .Select(tuple => new[] { tuple.Item1, tuple.Item2 });
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
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"{nameof(factory)} == null : {factory == null}");
            _outputHelper.WriteLine($"{nameof(expressionBuilder)} == null : {expressionBuilder == null}");

            // Act
            Action action = () => new RunCreatePatientCommand(factory, expressionBuilder);

            // Assert
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

            _expressionBuilderMock.Setup(mock => mock.GetMapExpression(typeof(CreatePatientInfo), typeof(Objects.Patient), It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((Type source, Type dest, IDictionary<string, object> parameters, MemberInfo[] membersToExpand) => AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.GetMapExpression(typeof(CreatePatientInfo), typeof(Objects.Patient), parameters, membersToExpand));

            _expressionBuilderMock.Setup(mock => mock.GetMapExpression(typeof(Objects.Patient), typeof(PatientInfo), It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((Type source, Type dest, IDictionary<string, object> parameters, MemberInfo[] membersToExpand) =>
                    AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.GetMapExpression(typeof(Objects.Patient), typeof(PatientInfo), parameters, membersToExpand));

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
            });

        }


    }

}

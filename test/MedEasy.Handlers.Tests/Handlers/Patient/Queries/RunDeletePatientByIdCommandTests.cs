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

namespace MedEasy.Handlers.Tests.Commands.Patient
{
    public class HandleDeletePatientByIdCommandTests : IDisposable
    {
        private Mock<ILogger<RunDeletePatientByIdCommand>> _loggerMock;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private Mock<IMapper> _mapperMock;
        private RunDeletePatientByIdCommand _handler;
        private Mock<IValidate<IDeletePatientByIdCommand>> _validatorMock;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private ITestOutputHelper _outputHelper;

        public static IEnumerable<object[]> ConstructorCases
        {
            get
            {

                yield return new object[]
                {
                    null,
                    Mock.Of<ILogger<RunDeletePatientByIdCommand>>(),
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<IExpressionBuilder>()
                };

                yield return new object[]
                {
                    Mock.Of<IValidate<IDeletePatientByIdCommand>>(),
                    null,
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<IExpressionBuilder>()
                };
                yield return new object[]
                {
                    Mock.Of<IValidate<IDeletePatientByIdCommand>>(),
                    Mock.Of<ILogger<RunDeletePatientByIdCommand>>(),
                    null,
                    Mock.Of<IExpressionBuilder>()
                };
                yield return new object[]
                {
                    Mock.Of<IValidate<IDeletePatientByIdCommand>>(),
                    Mock.Of<ILogger<RunDeletePatientByIdCommand>>(),
                    Mock.Of<IUnitOfWorkFactory>(),
                    null
                };


            }
        }
        public HandleDeletePatientByIdCommandTests(ITestOutputHelper outputHelper)
        {
            IMapper mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _loggerMock = new Mock<ILogger<RunDeletePatientByIdCommand>>(Strict);
            _mapperMock = new Mock<IMapper>(Strict);
            _validatorMock = new Mock<IValidate<IDeletePatientByIdCommand>>(Strict);
            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);

            _outputHelper = outputHelper;

            _handler = new RunDeletePatientByIdCommand(_validatorMock.Object, _loggerMock.Object, _unitOfWorkFactoryMock.Object,
               _expressionBuilderMock.Object);
        }


        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IValidate<IDeletePatientByIdCommand> validator, ILogger<RunDeletePatientByIdCommand> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Validator : {validator}");
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            Action action = () => new RunDeletePatientByIdCommand(validator, logger, factory, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactoryMock = null;
            _loggerMock = null;
            _mapperMock = null;
            _handler = null;
        }
    }
}

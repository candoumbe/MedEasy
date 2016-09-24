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

namespace MedEasy.BLL.Tests.Commands.Patient
{
    public class HandleCreatePatientCommandTests : IDisposable
    {
        private Mock<ILogger<RunCreatePatientCommand>> _loggerMock;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private Mock<IMapper> _mapperMock;
        private RunCreatePatientCommand _handler;
        private Mock<IValidate<ICreatePatientCommand>> _validatorMock;
        private Mock<IExpressionBuilder> _expressionBuilderMock;

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
        public HandleCreatePatientCommandTests(ITestOutputHelper output)
        {
            IMapper mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _loggerMock = new Mock<ILogger<RunCreatePatientCommand>>(Strict);
            _mapperMock = new Mock<IMapper>(Strict);
            _validatorMock = new Mock<IValidate<ICreatePatientCommand>>(Strict);
            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);
            _handler = new RunCreatePatientCommand(_validatorMock.Object, _loggerMock.Object, _unitOfWorkFactoryMock.Object, 
               _expressionBuilderMock.Object);
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


        public void Dispose()
        {
            _unitOfWorkFactoryMock = null;
            _loggerMock = null;
            _mapperMock = null;
            _handler = null;
        }
    }
}

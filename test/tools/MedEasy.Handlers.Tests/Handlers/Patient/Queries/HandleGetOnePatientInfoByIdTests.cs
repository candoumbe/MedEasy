using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MedEasy.DAL.Interfaces;
using MedEasy.RestObjects;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Handlers.Core.Patient.Queries;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Mapping;
using MedEasy.Queries;
using MedEasy.Queries.Patient;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using Moq;
using Optional;
using Queries.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;

namespace MedEasy.Handlers.Tests.Patient.Queries
{
    public class HandleGetOnePatientInfoByIdQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetPatientInfoByIdQuery _handler;

        private Mock<ILogger<HandleGetPatientInfoByIdQuery>> _loggerMock;
        private IMapper _mapper;
        private Mock<IValidator<IWantOneResource<Guid, Guid, PatientInfo>>> _validatorMock;

        public HandleGetOnePatientInfoByIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetPatientInfoByIdQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _validatorMock = new Mock<IValidator<IWantOneResource<Guid, Guid, PatientInfo>>>(Strict);

            _handler = new HandleGetPatientInfoByIdQuery(_validatorMock.Object, _loggerMock.Object, _unitOfWorkFactoryMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
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
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IValidator<IWantOneResource<Guid, Guid, PatientInfo>> validator, ILogger<HandleGetPatientInfoByIdQuery> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            Action action = () => new HandleGetPatientInfoByIdQuery(validator, logger, factory, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleGetOnePatientInfoByIdQuery handler = new HandleGetPatientInfoByIdQuery(
                    Mock.Of<IValidator<IWantOneResource<Guid, Guid, PatientInfo>>>(),
                    Mock.Of<ILogger<HandleGetPatientInfoByIdQuery>>(),
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<IExpressionBuilder>());

                await handler.HandleAsync(null);
            };

            action.ShouldThrow<ArgumentNullException>("because the query to handle is null")
                .And.ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task UnknownIdShouldReturnOptionNone()
        {
            //Arrange
            _validatorMock.Setup(mock => mock.ValidateAsync(It.IsAny<IWantOneResource<Guid, Guid, PatientInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult())
                .Verifiable();
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>()
                .SingleOrDefaultAsync(It.IsAny<Expression<Func<Objects.Patient, PatientInfo>>>(), It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Option<PatientInfo>>(Option.None<PatientInfo>()))
                .Verifiable();

            // Act
            Option<PatientInfo> output = await _handler.HandleAsync(new WantOnePatientInfoByIdQuery(Guid.NewGuid()));

            //Assert
            output.Should().Be(Option.None<PatientInfo>());

            _validatorMock.VerifyAll();
            _unitOfWorkFactoryMock.VerifyAll();
        }


        [Fact]
        public void ShouldThrowQueryNotValidExceptionIfValidationFails()
        {
            //Arrange
            _validatorMock.Setup(mock => mock.ValidateAsync(It.IsAny<IWantOneResource<Guid, Guid, PatientInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IWantOneResource<Guid, Guid, PatientInfo> q, CancellationToken cancellationToken) =>
                        new ValidationResult(new[]
                        {
                            new  ValidationFailure("PropName", "Error description")
                        }));

            // Act
            IWantOneResource<Guid, Guid, PatientInfo> query = new WantOnePatientInfoByIdQuery(Guid.NewGuid());
            Func<Task> action = async () => await _handler.HandleAsync(query);

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which
                .QueryId.Should().Be(query.Id);


        }




        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactoryMock = null;
            _handler = null;
            _mapper = null;
            _validatorMock = null;
        }
    }
}

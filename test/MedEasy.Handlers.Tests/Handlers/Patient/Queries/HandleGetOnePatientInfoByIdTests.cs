using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentAssertions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Handlers.Core.Patient.Queries;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Mapping;
using MedEasy.Queries;
using MedEasy.Queries.Patient;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.Validators.ErrorLevel;
using static Moq.MockBehavior;

namespace MedEasy.Handlers.Tests.Patient.Queries
{
    public class HandleGetOnePatientInfoByIdQueryTests: IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetPatientInfoByIdQuery _handler;
        
        private Mock<ILogger<HandleGetPatientInfoByIdQuery>> _loggerMock;
        private IMapper _mapper;
        private Mock<IValidate<IWantOneResource<Guid, int, PatientInfo>>> _validatorMock;

        public HandleGetOnePatientInfoByIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetPatientInfoByIdQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _validatorMock = new Mock<IValidate<IWantOneResource<Guid, int, PatientInfo>>>(Strict);

            _handler = new HandleGetPatientInfoByIdQuery(_validatorMock.Object, _loggerMock.Object,  _unitOfWorkFactoryMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
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
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IValidate<IWantOneResource<Guid, int, PatientInfo>> validator, ILogger<HandleGetPatientInfoByIdQuery> logger,
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
                    Mock.Of<IValidate<IWantOneResource<Guid, int, PatientInfo>>>(),
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
        public async Task UnknownIdShouldReturnNull()
        {
            //Arrange
            _validatorMock.Setup(mock => mock.Validate(It.IsAny<IWantOneResource<Guid, int, PatientInfo>>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>())
                .Verifiable();
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>()
                .SingleOrDefaultAsync(It.IsAny<Expression<Func<Objects.Patient, PatientInfo>>>(), It.IsAny<Expression<Func<Objects.Patient, bool>>>()))
                .ReturnsAsync(null)
                .Verifiable();

            // Act
            PatientInfo output = await _handler.HandleAsync(new WantOnePatientInfoByIdQuery(1));

            //Assert
            output.Should().BeNull();

            _validatorMock.VerifyAll();
            _unitOfWorkFactoryMock.VerifyAll();
        }


        [Fact]
        public void ShouldThrowQueryNotValidExceptionIfValidationFails()
        {
            //Arrange
            _validatorMock.Setup(mock => mock.Validate(It.IsAny<IWantOneResource<Guid, int, PatientInfo>>()))
                .Returns((IWantOneResource<Guid, int, PatientInfo> q) => new[] { Task.FromResult(new ErrorInfo("ErrCode", "A description", Error)) });

            // Act
            IWantOneResource<Guid, int, PatientInfo> query = new WantOnePatientInfoByIdQuery(1);
            Func<Task> action = async () =>  await _handler.HandleAsync(query);

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

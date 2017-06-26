using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using AutoMapper;
using Xunit;
using FluentAssertions;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Mapping;
using MedEasy.Handlers.Appointment.Queries;
using MedEasy.Queries.Appointment;
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.Handlers.Core.Appointment.Queries;
using System.Threading;
using Optional;

namespace MedEasy.Handlers.Tests.Appointment.Queries
{
    public class HandleGetOneAppointmentInfoByIdQueryTests: IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetAppointmentInfoByIdQuery _handler;
        
        private Mock<ILogger<HandleGetAppointmentInfoByIdQuery>> _loggerMock;
        private IMapper _mapper;

        public HandleGetOneAppointmentInfoByIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetAppointmentInfoByIdQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _handler = new HandleGetAppointmentInfoByIdQuery(_unitOfWorkFactoryMock.Object, _loggerMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
        }


        public static IEnumerable<object[]> ConstructorCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    null,
                    null
                };

            }
        }


        

        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(ILogger<HandleGetAppointmentInfoByIdQuery> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            Action action = () => new HandleGetAppointmentInfoByIdQuery(factory, logger, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleGetAppointmentInfoByIdQuery handler = new HandleGetAppointmentInfoByIdQuery(
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<ILogger<HandleGetAppointmentInfoByIdQuery>>(),
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
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Appointment>()
                .SingleOrDefaultAsync(It.IsAny<Expression<Func<Objects.Appointment, AppointmentInfo>>>(), It.IsAny<Expression<Func<Objects.Appointment, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Option<AppointmentInfo>>(Option.None<AppointmentInfo>()));

            // Act
            Option<AppointmentInfo> output = await _handler.HandleAsync(new WantOneAppointmentInfoByIdQuery(Guid.NewGuid()));

            //Assert
            output.HasValue.Should()
                .BeFalse();
        }
        



        public void Dispose()
        {
            _outputHelper = null;
           _unitOfWorkFactoryMock = null;
            _handler = null;
            _mapper = null;
        }
    }
}

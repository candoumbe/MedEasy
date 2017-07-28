﻿using System;
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
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Appointment.Queries;
using System.Threading;

namespace MedEasy.Handlers.Tests.Appointment.Queries
{
    public class HandleGetManyAppointmentInfosQueryTests: IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetManyAppointmentInfoQuery _handler;
        
        private Mock<ILogger<HandleGetManyAppointmentInfoQuery>> _loggerMock;
        private IMapper _mapper;

        public HandleGetManyAppointmentInfosQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetManyAppointmentInfoQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _handler = new HandleGetManyAppointmentInfoQuery(_unitOfWorkFactoryMock.Object, _loggerMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
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
        public async Task QueryAnEmptyDatabase()
        {
            //Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Appointment>()
                .ReadPageAsync(It.IsAny<Expression<Func<Objects.Appointment, AppointmentInfo>>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<OrderClause<AppointmentInfo>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<IPagedResult<AppointmentInfo>>(PagedResult<AppointmentInfo>.Default));

            // Act
            IPagedResult<AppointmentInfo> output = await _handler.HandleAsync(new GenericGetPageOfResourcesQuery<AppointmentInfo>(new PaginationConfiguration()));

            //Assert
            output.Should().NotBeNull();
            output.Entries.Should()
                .NotBeNull().And
                .BeEmpty();

            output.Total.Should().Be(0);


            _unitOfWorkFactoryMock.Verify();
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
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
using MedEasy.Handlers.Doctor.Queries;
using MedEasy.Queries.Doctor;
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.Handlers.Core.Doctor.Queries;
using System.Threading;
using Optional;

namespace MedEasy.Handlers.Tests.Doctor.Queries
{
    public class HandleGetOneDoctorInfoByIdQueryTests: IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetDoctorInfoByIdQuery _handler;
        
        private Mock<ILogger<HandleGetDoctorInfoByIdQuery>> _loggerMock;
        private IMapper _mapper;

        public HandleGetOneDoctorInfoByIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetDoctorInfoByIdQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _handler = new HandleGetDoctorInfoByIdQuery(_unitOfWorkFactoryMock.Object, _loggerMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
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
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(ILogger<HandleGetDoctorInfoByIdQuery> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            Action action = () => new HandleGetDoctorInfoByIdQuery(factory, logger, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleGetDoctorInfoByIdQuery handler = new HandleGetDoctorInfoByIdQuery(
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<ILogger<HandleGetDoctorInfoByIdQuery>>(),
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
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Doctor>()
                .SingleOrDefaultAsync(It.IsAny<Expression<Func<Objects.Doctor, DoctorInfo>>>(), It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Option<DoctorInfo>>(Option.None<DoctorInfo>()));

            // Act
            Option<DoctorInfo> output = await _handler.HandleAsync(new WantOneDoctorInfoByIdQuery(Guid.NewGuid()));

            //Assert
            output.Should()
                .Be(Option.None<DoctorInfo>());
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
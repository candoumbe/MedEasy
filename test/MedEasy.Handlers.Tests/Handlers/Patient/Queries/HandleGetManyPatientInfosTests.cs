﻿using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using MedEasy.Validators;
using AutoMapper;
using Xunit;
using FluentAssertions;
using MedEasy.Commands.Patient;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.DAL.Interfaces;
using MedEasy.Mapping;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Queries.Patient;
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MedEasy.Queries;

namespace MedEasy.Handlers.Tests.Patient.Queries
{
    public class HandleGetManyPatientInfosQueryTests: IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleGetManyPatientInfoQuery _handler;
        
        private Mock<ILogger<HandleGetManyPatientInfoQuery>> _loggerMock;
        private IMapper _mapper;

        public HandleGetManyPatientInfosQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleGetManyPatientInfoQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _handler = new HandleGetManyPatientInfoQuery(_unitOfWorkFactoryMock.Object, _loggerMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
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
            _outputHelper.WriteLine($"Validator : {validator}");
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
        public async Task QueryAnEmptyDatabase()
        {
            //Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>()
                .ReadPageAsync(It.IsAny<Expression<Func<Objects.Patient, PatientInfo>>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<OrderClause<PatientInfo>>>()))
                .ReturnsAsync(PagedResult<PatientInfo>.Default);

            // Act
            IPagedResult<PatientInfo> output = await _handler.HandleAsync(new GenericGetManyResourcesQuery<PatientInfo>(new GenericGetQuery()));

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
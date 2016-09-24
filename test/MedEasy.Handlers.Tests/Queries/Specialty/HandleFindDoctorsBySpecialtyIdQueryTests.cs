using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using MedEasy.Validators;
using AutoMapper;
using Xunit;
using FluentAssertions;
using MedEasy.Commands.Specialty;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MedEasy.Handlers.Specialty.Commands;
using MedEasy.DAL.Interfaces;
using MedEasy.Mapping;
using MedEasy.Handlers.Specialty.Queries;
using MedEasy.Queries.Specialty;
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MedEasy.Queries;
using System.Linq;

namespace MedEasy.Handlers.Tests.Specialty.Queries
{
    public class HandleFindDoctorsBySpecialtyIdQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private HandleFindDoctorsBySpecialtyIdQuery _handler;

        private Mock<ILogger<HandleFindDoctorsBySpecialtyIdQuery>> _loggerMock;


        public HandleFindDoctorsBySpecialtyIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;


            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleFindDoctorsBySpecialtyIdQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));


            _handler = new HandleFindDoctorsBySpecialtyIdQuery(_unitOfWorkFactoryMock.Object, _loggerMock.Object);
        }


        public static IEnumerable<object> CtorCases
        {
            get
            {
                yield return new object[] { null, null };
                yield return new object[] { new Mock<IUnitOfWorkFactory>().Object, null };
                yield return new object[] { null, new Mock<ILogger<HandleFindDoctorsBySpecialtyIdQuery>>().Object };
            }
        }


        public static IEnumerable<object> FindDoctorsBySpecialtyIdCases
        {
            get
            {
                yield return new object[]
                    {
                        new [] {
                            new Objects.Specialty { Id = 1, Code = "Spec 1", Name = "Specialty 1", Doctors = Enumerable.Empty<Objects.Doctor>().ToList() }
                        },
                        1,
                        new GenericGetQuery(),
                        1,
                        1,
                        0,
                        ((Expression<Func<IEnumerable<DoctorInfo>, bool>>) (items => items.Count() == 0))

                };
            }
        }




        [Theory]
        [MemberData(nameof(CtorCases))]
        public void ShouldThrowArgumentNullException(IUnitOfWorkFactory factory, ILogger<HandleFindDoctorsBySpecialtyIdQuery> logger)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            Action action = () => new HandleFindDoctorsBySpecialtyIdQuery(factory, logger);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void HandlingNullQueryShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleFindDoctorsBySpecialtyIdQuery handler = new HandleFindDoctorsBySpecialtyIdQuery(
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<ILogger<HandleFindDoctorsBySpecialtyIdQuery>>());

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
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Specialty>()
                .ReadPageAsync(It.IsAny<Expression<Func<Objects.Specialty, SpecialtyInfo>>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<OrderClause<SpecialtyInfo>>>()))
                .Returns((Expression<Func<Objects.Specialty, SpecialtyInfo>> selector, int pageSize, int page, IEnumerable<OrderClause<SpecialtyInfo>> order)
                => Task.FromResult<IPagedResult<SpecialtyInfo>>(new PagedResult<SpecialtyInfo>(Enumerable.Empty<SpecialtyInfo>(), 0, pageSize)));

            // Act

            IPagedResult<DoctorInfo> output = await _handler.HandleAsync(new FindDoctorsBySpecialtyIdQuery(1, new GenericGetQuery()));

            //Assert
            output.Should().NotBeNull();
            output.Entries.Should()
                .NotBeNull().And
                .BeEmpty();

            output.Total.Should().Be(0);
            output.PageCount.Should().Be(0);
            output.PageSize.Should().Be(GenericGetQuery.DefaultPageSize);


            _unitOfWorkFactoryMock.Verify();
        }


        [Theory]
        [MemberData(nameof(FindDoctorsBySpecialtyIdCases))]
        public async Task FindDoctorsBySpecialtyId(IEnumerable<Objects.Specialty> specialties, int specialtyId, GenericGetQuery getQuery, int expectedPageSize,
            int expectedPage, int expectedTotal, Expression<Func<IEnumerable<DoctorInfo>, bool>> itemsExpectation)
        {
            // Arrange

            
            // Act
            IPagedResult<DoctorInfo> pageOfResult = await _handler.HandleAsync(new FindDoctorsBySpecialtyIdQuery(specialtyId, getQuery));

            // Assert
            pageOfResult.Should().NotBeNull();
            pageOfResult.Entries.Should()
                .NotBeNull().And
                .Match(itemsExpectation);

            pageOfResult.Total.Should().Be(expectedTotal);


            _unitOfWorkFactoryMock.Verify();


        }


        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactoryMock = null;
            _handler = null;
        }
    }
}

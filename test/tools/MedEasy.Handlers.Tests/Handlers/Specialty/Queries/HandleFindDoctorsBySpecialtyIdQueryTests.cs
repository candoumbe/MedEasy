using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Specialty.Queries;
using MedEasy.Queries.Specialty;
using MedEasy.RestObjects;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System.Linq;
using AutoMapper.QueryableExtensions;
using MedEasy.Mapping;
using static Newtonsoft.Json.JsonConvert;
using MedEasy.Handlers.Core.Specialty.Queries;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;
using System.Reflection;
using Optional;

namespace MedEasy.Handlers.Tests.Specialty.Queries
{
    public class HandleFindDoctorsBySpecialtyIdQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private HandleFindDoctorsBySpecialtyIdQuery _handler;

        private Mock<ILogger<HandleFindDoctorsBySpecialtyIdQuery>> _loggerMock;
        private IExpressionBuilder _expressionBuilder;

        public HandleFindDoctorsBySpecialtyIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;


            DbContextOptionsBuilder<MedEasyContext> dbOptionsBuiler = new DbContextOptionsBuilder<MedEasyContext>();
            dbOptionsBuiler.UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(dbOptionsBuiler.Options);

            _loggerMock = new Mock<ILogger<HandleFindDoctorsBySpecialtyIdQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
            _expressionBuilder = AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder;

            _handler = new HandleFindDoctorsBySpecialtyIdQuery(_unitOfWorkFactory, _loggerMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactory = null;
            _handler = null;
            _expressionBuilder = null;
            _loggerMock = null;
        }

        public static IEnumerable<object> CtorCases
        {
            get
            {
                yield return new object[] { null, null, null };
                yield return new object[] { new Mock<IUnitOfWorkFactory>().Object, null, null };
                yield return new object[] { null, new Mock<ILogger<HandleFindDoctorsBySpecialtyIdQuery>>().Object, null };
                yield return new object[] { null, null, new Mock<IExpressionBuilder>().Object };
            }
        }


        public static IEnumerable<object> FindDoctorsBySpecialtyIdCases
        {
            get
            {
                
                yield return new object[]
                {
                    new [] {
                        new Objects.Doctor {
                            Id = 1, Firstname = "Henry", Lastname = "Jekyll", Specialty = new Objects.Specialty { Id = 1, Name = "SPEC1", UUID = Guid.NewGuid() }
                        }
                    },
                    Guid.NewGuid(),
                    new PaginationConfiguration(),
                    ((Action<Option<IPagedResult<DoctorInfo>>>) (items => items.HasValue.Should().BeFalse()))

                };
                

                {
                    Guid specialtyId = Guid.NewGuid();
                    Objects.Specialty specialty = new Objects.Specialty { Id = 1, Name = "SPEC1", UUID = specialtyId };
                    Guid doctorId = Guid.NewGuid();
                    Guid doctorId2 = Guid.NewGuid();
                    yield return new object[]
                    {
                        new [] {
                            new Objects.Doctor {Id = 1, Firstname = "Henry", Lastname = "Jekyll", UUID = doctorId, Specialty = specialty },
                            new Objects.Doctor {Id = 2, Firstname = "Hugo", Lastname = "Strange", UUID = doctorId2, Specialty = specialty }
                        },
                        specialtyId,
                        new PaginationConfiguration {Page = 1, PageSize = 1 },
                        ((Action<Option<IPagedResult<DoctorInfo>>>) (items => {
                            items.HasValue.Should().BeTrue();
                            items.MatchSome(
                                pageOfResult =>
                                {
                                    pageOfResult.Should()
                                        .NotBeNull();
                                    pageOfResult.Entries.Should().HaveCount(1).And
                                        .Contain(x => x.Id == doctorId);
                                });
                        }))
                    };
                }
            }
        }




        [Theory]
        [MemberData(nameof(CtorCases))]
        public void ShouldThrowArgumentNullException(IUnitOfWorkFactory factory, ILogger<HandleFindDoctorsBySpecialtyIdQuery> logger, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"ExpressionBuilder : {expressionBuilder}");
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

            

            // Act
            Option<IPagedResult<DoctorInfo>> output = await _handler.HandleAsync(new FindDoctorsBySpecialtyIdQuery(Guid.NewGuid(), new PaginationConfiguration()));

            //Assert
            output.HasValue.Should().BeFalse();
            

        }


        [Theory]
        [MemberData(nameof(FindDoctorsBySpecialtyIdCases))]
        public async Task FindDoctorsBySpecialtyId(IEnumerable<Objects.Doctor> doctors, Guid specialtyId, PaginationConfiguration getQuery, Action<Option<IPagedResult<DoctorInfo>>> assertions)
        {

            _outputHelper.WriteLine($"{nameof(doctors)} : {SerializeObject(doctors)}");
            _outputHelper.WriteLine($"{nameof(specialtyId)} : {specialtyId}");


            // Arrange
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Objects.Doctor>().Create(doctors);

                await uow.SaveChangesAsync();
            }

            // Act
            Option<IPagedResult<DoctorInfo>> pageOfResult = await _handler.HandleAsync(new FindDoctorsBySpecialtyIdQuery(specialtyId, getQuery));

            // Assert
            assertions(pageOfResult);

        }



    }
}

using AutoMapper.QueryableExtensions;
using FluentAssertions;
using GenFu;
using MedEasy.Commands;
using MedEasy.Commands.Patient;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Exceptions;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Mapping;
using MedEasy.Objects;
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
using static Moq.MockBehavior;

namespace MedEasy.Services.Tests
{
    public class PhysiologicalMeasureServiceTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private PhysiologicalMeasureService _physiologicalMeasureService;
        private Mock<IValidate<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>> _iValidateDeleteOnePhysiologicalMeasureCommandMock;
        private Mock<ILogger<PhysiologicalMeasureService>> _loggerMock;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private IExpressionBuilder _expressionBuilder;

        public PhysiologicalMeasureServiceTests(ITestOutputHelper outputHelper)
        {

            _outputHelper = outputHelper;


            _iValidateDeleteOnePhysiologicalMeasureCommandMock = new Mock<IValidate<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>>(Strict);

            _loggerMock = new Mock<ILogger<PhysiologicalMeasureService>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose())
                .Verifiable("unit of work must be in a using block");

            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;

            _physiologicalMeasureService = new PhysiologicalMeasureService(
                _unitOfWorkFactoryMock.Object,
                _loggerMock.Object,
                _iValidateDeleteOnePhysiologicalMeasureCommandMock.Object,
                _expressionBuilder);
        }

        public static IEnumerable<object> GetMostRecentMeasuresCases
        {
            get
            {
                yield return new object[] {
                    Enumerable.Empty<BloodPressure>(),
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>)(x => !x.Any()))
                };

                yield return new object[] {
                    new [] {
                        new BloodPressure { PatientId = 1, SystolicPressure = 120, DiastolicPressure = 80 },
                        new BloodPressure { PatientId = 2, SystolicPressure = 120, DiastolicPressure = 80 }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>)(x => x.Count() == 1))
                };

                yield return new object[] {
                    new [] {
                        new BloodPressure { PatientId = 2, SystolicPressure = 120, DiastolicPressure = 80 },
                        new BloodPressure { PatientId = 2, SystolicPressure = 128, DiastolicPressure = 95 },
                        new BloodPressure { PatientId = 2, SystolicPressure = 130, DiastolicPressure = 95 }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = 2, Count = 2 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>)(x => x.Count() == 2))
                };
            }
        }

        public static IEnumerable<object> GetOneMeasureAsyncCases
        {
            get
            {
                yield return new object[] {
                    Enumerable.Empty<BloodPressure>(),
                    new GetOnePhysiologicalMeasureInfo { PatientId = 1, MeasureId = 10 },
                    ((Expression<Func<BloodPressureInfo, bool>>)(x => x == null))
                };

                yield return new object[] {
                    new [] {
                        new BloodPressure { Id = 10, PatientId = 1, SystolicPressure = 120, DiastolicPressure = 80 },
                        new BloodPressure { Id = 20, PatientId = 2, SystolicPressure = 120, DiastolicPressure = 80 }
                    },
                    new GetOnePhysiologicalMeasureInfo { PatientId = 1, MeasureId = 10 },
                    ((Expression<Func<BloodPressureInfo, bool>>)(x => x.PatientId == 1 && x.Id == 10))
                };
            }
        }

        [Fact]
        public async Task AddNewBloodPressureResource()
        {
            // Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<BloodPressure>().Create(It.IsAny<BloodPressure>()))
                .Returns((BloodPressure measure) =>
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    measure.Id = 1;
                    measure.CreatedDate = now;
                    measure.UpdatedDate = now;

                    return measure;
                });
            _unitOfWorkFactoryMock.Setup(mock => mock.New().SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            // Act
            BloodPressure input = new BloodPressure
            {
                PatientId = 1,
                DateOfMeasure = DateTimeOffset.UtcNow,
                SystolicPressure = 120,
                DiastolicPressure = 80
            };
            BloodPressureInfo output = await _physiologicalMeasureService.AddNewMeasureAsync<BloodPressure, BloodPressureInfo>(new AddNewPhysiologicalMeasureCommand<BloodPressure, BloodPressureInfo>(input));

            // Assert
            output.Should().NotBeNull();
            output.PatientId.Should().Be(input.Id);
            output.DateOfMeasure.Should().Be(input.DateOfMeasure);
            output.SystolicPressure.Should().Be(input.SystolicPressure);
            output.DiastolicPressure.Should().Be(input.DiastolicPressure);


            _iValidateDeleteOnePhysiologicalMeasureCommandMock.Verify();
            _unitOfWorkFactoryMock.VerifyAll();
            _loggerMock.VerifyAll();

        }


        [Theory]
        [MemberData(nameof(GetMostRecentMeasuresCases))]
        public async Task GetMostRecentMeasures(IEnumerable<BloodPressure> measuresBdd, GetMostRecentPhysiologicalMeasuresInfo query, Expression<Func<IEnumerable<BloodPressureInfo>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"Current store state : {measuresBdd}");
            _outputHelper.WriteLine($"Query : {query}");
            // Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<BloodPressure>()
                .WhereAsync(
                    It.IsAny<Expression<Func<BloodPressure, BloodPressureInfo>>>(),
                    It.IsAny<Expression<Func<BloodPressure, bool>>>(),
                    It.IsAny<IEnumerable<OrderClause<BloodPressureInfo>>>(),
                    It.IsAny<int>(), It.IsAny<int>()))
                .Returns((Expression<Func<BloodPressure, BloodPressureInfo>> selector, Expression<Func<BloodPressure, bool>> filter,
                    IEnumerable<OrderClause<BloodPressureInfo>> sorts, int pageSize, int page
                ) => Task.Run(() =>
                {

                    IEnumerable<BloodPressureInfo> results = measuresBdd.Where(filter.Compile())
                        .Select(selector.Compile())
                        .AsQueryable()
                        .OrderBy(sorts)
                        .Skip(page < 1 ? pageSize : page - 1 * pageSize)
                        .Take(pageSize)
                        .ToArray();

                    return (IPagedResult<BloodPressureInfo>)new PagedResult<BloodPressureInfo>(results, measuresBdd.Count(filter.Compile()), pageSize);
                }));

            // Act
            IEnumerable<BloodPressureInfo> measures = await _physiologicalMeasureService.GetMostRecentMeasuresAsync<BloodPressure, BloodPressureInfo>(new WantMostRecentPhysiologicalMeasuresQuery<BloodPressureInfo>(query));

            // Assert
            measures.Should().NotBeNull().And.Match(resultExpectation);
            _unitOfWorkFactoryMock.VerifyAll();
            _loggerMock.VerifyAll();
        }


        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            // Act
            Func<Task> action = async () => await _physiologicalMeasureService.GetOneMeasureAsync<BloodPressure, BloodPressureInfo>(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>("the query is null").Which
                .ParamName.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [MemberData(nameof(GetOneMeasureAsyncCases))]
        public async Task GetOneMeasureAsync(IEnumerable<BloodPressure> measuresBdd, GetOnePhysiologicalMeasureInfo query, Expression<Func<BloodPressureInfo, bool>> resultExpectation)
        {
            // Arrange
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<BloodPressure>()
                .SingleOrDefaultAsync(
                    It.IsAny<Expression<Func<BloodPressure, BloodPressureInfo>>>(),
                    It.IsAny<Expression<Func<BloodPressure, bool>>>()))
                .Returns((Expression<Func<BloodPressure, BloodPressureInfo>> selector, Expression<Func<BloodPressure, bool>> filter)

                 => Task.Run(() =>
                {
                    return measuresBdd.Where(filter.Compile()).Select(selector.Compile()).SingleOrDefault();
                }));

            // Act
            BloodPressureInfo measure = await _physiologicalMeasureService.GetOneMeasureAsync<BloodPressure, BloodPressureInfo>(new WantOnePhysiologicalMeasureQuery<BloodPressureInfo>(query.PatientId, query.MeasureId));

            // Assert
            measure.Should().Match(resultExpectation);
            _unitOfWorkFactoryMock.VerifyAll();
            _loggerMock.VerifyAll();
        }


        [Fact]
        public void ShouldThrowArgumentNullExceptionWhenQueryIsNull()
        {

            // Act
            Func<Task> action = async () => await _physiologicalMeasureService.GetMostRecentMeasuresAsync<BloodPressure, BloodPressureInfo>(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>("query cannot be null").Which
                .ParamName.Should().NotBeNullOrWhiteSpace("paramName must be set for easier debugging");
        }



        [Fact]
        public async Task DeleteOneBloodPressureResource()
        {
            // Arrange
            _iValidateDeleteOnePhysiologicalMeasureCommandMock.Setup(mock => mock.Validate(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>());
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<BloodPressure>().Delete(It.IsAny<Expression<Func<BloodPressure, bool>>>()))
                .Verifiable();
            _unitOfWorkFactoryMock.Setup(mock => mock.New().SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            // Act
            await _physiologicalMeasureService.DeleteOnePhysiologicalMeasureAsync<BloodPressure>(new DeleteOnePhysiologicalMeasureCommand(new DeletePhysiologicalMeasureInfo { Id = 1, MeasureId = 3 }));

            // Assert
            _iValidateDeleteOnePhysiologicalMeasureCommandMock.Verify();
            _unitOfWorkFactoryMock.VerifyAll();
            _loggerMock.VerifyAll();

        }


        public void Dispose()
        {
            _iValidateDeleteOnePhysiologicalMeasureCommandMock = null;
            _loggerMock = null;
            _outputHelper = null;
            _unitOfWorkFactoryMock = null;
            _physiologicalMeasureService = null;
        }
    }
}

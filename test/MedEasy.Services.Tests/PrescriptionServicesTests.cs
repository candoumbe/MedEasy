using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using FluentAssertions;
using MedEasy.DTO;
using MedEasy.Objects;
using System.Linq.Expressions;
using Xunit;
using MedEasy.Mapping;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Exceptions;
using MedEasy.DAL.Repositories;

namespace MedEasy.Services.Tests
{
    public class PrescriptionServicesTests : IDisposable
    {
        private Mock<IUnitOfWorkFactory> _factoryMock;
        private Mock<ILogger<PrescriptionService>> _loggerMock;
        private ITestOutputHelper _outputHelper;
        private PrescriptionService _service;
        private IMapper _mapper;
        private IExpressionBuilder _expressionBuilder;

        public PrescriptionServicesTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _loggerMock = new Mock<ILogger<PrescriptionService>>(Strict);

            _factoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _factoryMock.Setup(mock => mock.New().Dispose());

            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;
            _service = new PrescriptionService(_factoryMock.Object, _loggerMock.Object, _expressionBuilder);
        }

        public static IEnumerable<object> GetOnePrescriptionByPatientIdAsyncCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Prescription>(),
                    1, 2,
                    ((Expression<Func<PrescriptionHeaderInfo, bool>>)(x => x  == null))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Prescription
                        {
                            Id = 2,
                            PatientId = 1,
                            DeliveryDate = 23.December(2000)
                        }
                    },
                    1, 2,
                    ((Expression<Func<PrescriptionHeaderInfo, bool>>)(x => x  != null && x.DeliveryDate == 23.December(2000) && x.Id == 2 && x.PatientId == 1))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Prescription
                        {
                            Id = 3,
                            PatientId = 1,
                            DeliveryDate = 23.December(2000)
                        }
                    },
                    1, 2,
                    ((Expression<Func<PrescriptionHeaderInfo, bool>>)(x => x  == null))
                };
            }
        }

        public static IEnumerable<object> GetOnePrescriptionTestsCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Prescription>(),
                    1, 
                    ((Expression<Func<PrescriptionHeaderInfo, bool>>)(x => x  == null))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Prescription
                        {
                            Id = 2,
                            PatientId = 1,
                            DeliveryDate = 23.December(2000)
                        }
                    },
                    2,
                    ((Expression<Func<PrescriptionHeaderInfo, bool>>)(x => x  != null && x.DeliveryDate == 23.December(2000) && x.Id == 2 && x.PatientId == 1))
                };
            }
        }


        public void Dispose()
        {
            _expressionBuilder = null;
            _outputHelper = null;

            _loggerMock = null;

            _factoryMock = null;

            _service = null;
        }

        [Fact]
        public async Task CreateNewPrescriptionForPatient()
        {
            // Arrange
            _factoryMock.Setup(mock => mock.New().Repository<Prescription>().Create(It.IsAny<Prescription>()))
                .Returns((Prescription input) =>
                {
                    input.Id = 1;

                    return input;
                });

            _factoryMock.Setup(mock => mock.New().Repository<Patient>().AnyAsync(It.IsAny<Expression<Func<Patient, bool>>>()))
                .ReturnsAsync(true);

            _factoryMock.Setup(mock => mock.New().SaveChangesAsync()).ReturnsAsync(1);
            
            // Act
            CreatePrescriptionInfo newPrescription = new CreatePrescriptionInfo
            {
                DeliveryDate = 23.June(2005),
                Duration = TimeSpan.FromDays(30).TotalDays,
                Items = new[]
                {
                    new PrescriptionItemInfo { Code = "DRUG",  Quantity = 1m  }
                }
            };
            PrescriptionHeaderInfo prescriptionHeader = await _service.CreatePrescriptionForPatientAsync(1, newPrescription);


            // Assert

            prescriptionHeader.Should().NotBeNull();
            prescriptionHeader.DeliveryDate.Should().Be(newPrescription.DeliveryDate);
            prescriptionHeader.PatientId.Should().Be(1, "id of the patient must match the first argument");

            _factoryMock.Verify();
            _loggerMock.VerifyAll();
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionWhenPrescriptionIsNull()
        {
            // Act
            Func<Task> action = async () => await _service.CreatePrescriptionForPatientAsync(1, null);


            // Assert

            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldThrowArgumentOutOfRangeExceptionWhenPatientIdIsNegativeOrNull(int patientId)
        {
            // Act
            CreatePrescriptionInfo newPrescription = new CreatePrescriptionInfo
            {
                DeliveryDate = 23.June(2005),
                Duration = TimeSpan.FromDays(30).TotalDays,
                Items = new[]
                {
                    new PrescriptionItemInfo { Code = "DRUG",  Quantity = 1m  }
                }
            };

            Func<Task> action = async () => await _service.CreatePrescriptionForPatientAsync(patientId, newPrescription);


            // Assert

            action.ShouldThrow<ArgumentOutOfRangeException>($"{nameof(patientId)} is {patientId}").Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Theory]
        [MemberData(nameof(GetOnePrescriptionByPatientIdAsyncCases))]
        public async Task GetOnePrescriptionByPatientIdAsync(IEnumerable<Prescription> prescriptionsBdd, int patientId, int prescriptionId, Expression<Func<PrescriptionHeaderInfo, bool>> resultExpectation)
        {
            // Arrange 
            _factoryMock.Setup(mock => mock.New().Repository<Prescription>().SingleOrDefaultAsync(It.IsAny<Expression<Func<Prescription, PrescriptionHeaderInfo>>>(), It.IsAny<Expression<Func<Prescription, bool>>>()))
                .Returns((Expression<Func<Prescription, PrescriptionHeaderInfo>> selector, Expression<Func<Prescription, bool>> filter) => Task.Run(() =>
                {
                    return prescriptionsBdd
                        .Where(filter.Compile())
                        .Select(selector.Compile())
                        .SingleOrDefault();
                }));

            // Act
            PrescriptionHeaderInfo output = await _service.GetOnePrescriptionByPatientIdAsync(patientId, prescriptionId);

            // Assert
            output.Should().Match(resultExpectation);

        }

        [Fact]
        public void GetDetailsByPrescriptionIdThrowsNotFoundExceptionWhenPrescriptionIdNotFound()
        {

            // Arrange
            _factoryMock.Setup(mock => mock.New().Repository<Prescription>().SingleOrDefaultAsync(It.IsAny<Expression<Func<Prescription, bool>>>(), It.IsAny<IEnumerable<IncludeClause<Prescription>>>()))
                .ReturnsAsync(null);

            // Act
            Func<Task> action = async () => await _service.GetItemsByPrescriptionIdAsync(1);

            // Assert
            action.ShouldThrow<NotFoundException>()
                .Which.Message.Should()
                .MatchRegex(@"Prescription <\d+> not found");
        }

        

        [Theory]
        [MemberData(nameof(GetOnePrescriptionTestsCases))]
        public async Task GetOnePrescriptionTests(IEnumerable<Prescription> prescriptionsBdd, int id, Expression<Func<PrescriptionHeaderInfo, bool>> resultExpectation)
        {
            // Arrange 
            _factoryMock.Setup(mock => mock.New().Repository<Prescription>().SingleOrDefaultAsync(It.IsAny<Expression<Func<Prescription, PrescriptionHeaderInfo>>>(), It.IsAny<Expression<Func<Prescription, bool>>>()))
                .Returns((Expression<Func<Prescription, PrescriptionHeaderInfo>> selector, Expression<Func<Prescription, bool>> filter) => Task.Run(() =>
                {
                    return prescriptionsBdd
                        .Where(filter.Compile())
                        .Select(selector.Compile())
                        .SingleOrDefault();
                }));

            // Act
            PrescriptionHeaderInfo output = await _service.GetOnePrescriptionAsync(id);

            // Assert
            output.Should().Match(resultExpectation);

        }


        /// <summary>
        /// Tests that <see cref="PrescriptionService.GetOnePrescriptionByPatientIdAsync(int, int)"/> throws <see cref="ArgumentOutOfRangeException"/>
        /// if any parameter is negative or zero
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="prescriptionId"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(-1, -1)]
        [InlineData(0, -1)]
        [InlineData(0, -1)]
        public async Task ShouldThrowArgumentOutOfRangeException(int patientId, int prescriptionId)
        {
            // Act
            Func<Task> action = async () =>  await _service.GetOnePrescriptionByPatientIdAsync(patientId, prescriptionId);

            // Assert
            action.ShouldThrow<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();

        }
    }
}

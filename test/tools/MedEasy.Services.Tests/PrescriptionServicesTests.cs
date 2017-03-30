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
using MedEasy.Handlers.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Services.Tests
{
    public class PrescriptionServicesTests : IDisposable
    {
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private Mock<ILogger<PrescriptionService>> _loggerMock;
        private ITestOutputHelper _outputHelper;
        private PrescriptionService _service;
        private IMapper _mapper;

        public PrescriptionServicesTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _loggerMock = new Mock<ILogger<PrescriptionService>>(Strict);

            DbContextOptionsBuilder<MedEasyContext> builder = new DbContextOptionsBuilder<MedEasyContext>()
                .UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(builder.Options);
            
            _mapper = AutoMapperConfig.Build().CreateMapper();
            _service = new PrescriptionService(_unitOfWorkFactory, _loggerMock.Object, _mapper);
        }

        public void Dispose()
        {
            _mapper = null;
            _outputHelper = null;

            _loggerMock = null;

            _unitOfWorkFactory = null;

            _service = null;
        }


        public static IEnumerable<object> GetOnePrescriptionByPatientIdAsyncCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Prescription>(),
                    Guid.NewGuid(), Guid.NewGuid(),
                    ((Expression<Func<PrescriptionHeaderInfo, bool>>)(x => x  == null))
                };

                {
                    Guid patientId = Guid.NewGuid();
                    Guid prescriptionId = Guid.NewGuid();
                    Patient p = new Patient
                    {

                        UUID = patientId
                    };
                    yield return new object[]
                    {
                        new []
                        {
                            new Prescription
                            {
                                Id = 2,
                                Patient = p,
                                DeliveryDate = 23.December(2000),
                                UUID = prescriptionId
                            }
                        },
                        patientId, prescriptionId,
                        ((Expression<Func<PrescriptionHeaderInfo, bool>>)(x => x  != null && x.DeliveryDate == 23.December(2000) && x.Id == prescriptionId && x.PatientId == patientId))
                    };
                }

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
                    Guid.NewGuid(), Guid.NewGuid(),
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
                    Guid.NewGuid(), 
                    ((Expression<Func<PrescriptionHeaderInfo, bool>>)(x => x == null))
                };
                {
                    Guid prescriptionId = Guid.NewGuid();
                    Guid patientId = Guid.NewGuid();
                    Patient p = new Patient
                    {
                        Id = 1,
                        UUID = patientId
                    };
                    yield return new object[]
                    {
                        new []
                        {
                            new Prescription
                            {
                                Id = 2,
                                DeliveryDate = 23.December(2000),
                                Patient = p,
                                PatientId = p.Id,
                                UUID = prescriptionId
                            }
                        },
                        prescriptionId,
                        ((Expression<Func<PrescriptionHeaderInfo, bool>>)(x => x  != null && x.DeliveryDate == 23.December(2000) && x.Id == prescriptionId && x.PatientId == patientId))
                    };
                }
            }
        }

        [Fact]
        public async Task CreateNewPrescriptionForPatient()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            Guid prescriptorId = Guid.NewGuid();

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Patient>().Create(new Patient { UUID = patientId });
                uow.Repository<Doctor>().Create(new Doctor { UUID = prescriptorId });

                await uow.SaveChangesAsync();
            }

            // Act
            CreatePrescriptionInfo newPrescription = new CreatePrescriptionInfo
            {
                DeliveryDate = 23.June(2005),
                Duration = TimeSpan.FromDays(30).TotalDays,
                PrescriptorId = prescriptorId,
                Items = new[]
                {
                    new PrescriptionItemInfo { Code = "DRUG",  Quantity = 1m  }
                }
            };
            PrescriptionHeaderInfo prescriptionHeader = await _service.CreatePrescriptionForPatientAsync(patientId, newPrescription);


            // Assert

            prescriptionHeader.Should().NotBeNull();
            prescriptionHeader.DeliveryDate.Should().Be(newPrescription.DeliveryDate);
            prescriptionHeader.PatientId.Should().Be(patientId, "id of the patient must match the first argument");

            _loggerMock.VerifyAll();
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionWhenPrescriptionIsNull()
        {
            // Act
            Func<Task> action = async () => await _service.CreatePrescriptionForPatientAsync(Guid.NewGuid(), null);


            // Assert

            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ShouldThrowArgumentOutOfRangeExceptionWhenPatientIdIsNegativeOrNull()
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

            Func<Task> action = async () => await _service.CreatePrescriptionForPatientAsync(Guid.Empty, newPrescription);


            // Assert

            action.ShouldThrow<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Theory]
        [MemberData(nameof(GetOnePrescriptionByPatientIdAsyncCases))]
        public async Task GetOnePrescriptionByPatientIdAsync(IEnumerable<Prescription> prescriptionsBdd, Guid patientId, Guid prescriptionId, Expression<Func<PrescriptionHeaderInfo, bool>> resultExpectation)
        {
            // Arrange 
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Prescription>().Create(prescriptionsBdd);

                await uow.SaveChangesAsync();
            }

            // Act
            PrescriptionHeaderInfo output = await _service.GetOnePrescriptionByPatientIdAsync(patientId, prescriptionId);

            // Assert
            output.Should().Match(resultExpectation);

        }

        [Fact]
        public void GetDetailsByPrescriptionIdThrowsNotFoundExceptionWhenPrescriptionIdNotFound()
        {

            // Arrange

            // Act
            Guid prescriptionId = Guid.NewGuid();
            Func<Task> action = async () => await _service.GetItemsByPrescriptionIdAsync(prescriptionId);

            // Assert
            action.ShouldThrow<NotFoundException>()
                .Which.Message.Should()
                .Be($"Prescription <{prescriptionId}> not found");
        }

        

        [Theory]
        [MemberData(nameof(GetOnePrescriptionTestsCases))]
        public async Task GetOnePrescriptionTests(IEnumerable<Prescription> prescriptionsBdd, Guid id, Expression<Func<PrescriptionHeaderInfo, bool>> resultExpectation)
        {
            // Arrange 
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Prescription>().Create(prescriptionsBdd);

                await uow.SaveChangesAsync();
            }

            _outputHelper.WriteLine($"Prescriptions : {SerializeObject(prescriptionsBdd, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");

            // Act
            PrescriptionHeaderInfo output = await _service.GetOnePrescriptionAsync(id);

            // Assert
            output.Should().Match(resultExpectation);

        }


        /// <summary>
        /// Tests that <see cref="PrescriptionService.GetOnePrescriptionByPatientIdAsync(int, int)"/> throws <see cref="ArgumentOutOfRangeException"/>
        /// if any parameter is empty
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="prescriptionId"></param>
        /// <returns></returns>
        [Fact]
        public void ShouldThrowArgumentOutOfRangeException()
        {
            // Act
            Func<Task> action = async () =>  await _service.GetOnePrescriptionByPatientIdAsync(Guid.Empty, Guid.Empty);

            // Assert
            action.ShouldThrow<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();

        }
    }
}

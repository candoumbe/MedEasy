using AutoMapper;
using FluentAssertions;
using MedEasy.API.Stores;
using MedEasy.Commands;
using MedEasy.Commands.Patient;
using MedEasy.DAL.Interfaces;
using MedEasy.RestObjects;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.Queries.Patient;
using MedEasy.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Newtonsoft.Json.JsonConvert;
using static Moq.MockBehavior;
using Optional;
using FluentValidation;
using FluentValidation.Results;
using System.Threading;

namespace MedEasy.Services.Tests
{
    public class PhysiologicalMeasureServiceTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private PhysiologicalMeasureService _physiologicalMeasureService;
        private Mock<IValidator<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>> _iValidateDeleteOnePhysiologicalMeasureCommandMock;
        private Mock<ILogger<PhysiologicalMeasureService>> _loggerMock;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IMapper _mapper;

        public PhysiologicalMeasureServiceTests(ITestOutputHelper outputHelper)
        {

            _outputHelper = outputHelper;


            _iValidateDeleteOnePhysiologicalMeasureCommandMock = new Mock<IValidator<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>>(Strict);

            _loggerMock = new Mock<ILogger<PhysiologicalMeasureService>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
            DbContextOptionsBuilder<MedEasyContext> builder = new DbContextOptionsBuilder<MedEasyContext>()
                .UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(builder.Options);


            _mapper = AutoMapperConfig.Build().CreateMapper();

            _physiologicalMeasureService = new PhysiologicalMeasureService(
                _unitOfWorkFactory,
                _loggerMock.Object,
                _iValidateDeleteOnePhysiologicalMeasureCommandMock.Object,
                _mapper);
        }

        public static IEnumerable<object> GetMostRecentMeasuresCases
        {
            get
            {
                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[] {
                        Enumerable.Empty<Patient>(),
                        Enumerable.Empty<BloodPressure>(),
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 10 },
                        ((Expression<Func<Option<IEnumerable<BloodPressureInfo>>, bool>>)(x => !x.HasValue ))
                    };
                }

                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[] {
                        new [] { new Patient { UUID = patientId } },
                        Enumerable.Empty<BloodPressure>(),
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 10 },
                        ((Expression<Func<Option<IEnumerable<BloodPressureInfo>>, bool>>)(x => x.HasValue && x.Exists(items => !items.Any())))
                    };
                }

                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[] {
                        new[]
                        {
                            new Patient { Id = 1, UUID = patientId }
                        },
                        new [] {
                            new BloodPressure { SystolicPressure = 120, DiastolicPressure = 80, Patient = new Patient { Id = 1, UUID = patientId } },
                            new BloodPressure { SystolicPressure = 120, DiastolicPressure = 80, Patient = new Patient { Id = 2, UUID = Guid.NewGuid() }  }
                        },
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 10 },
                        ((Expression<Func<Option<IEnumerable<BloodPressureInfo>>, bool>>)(x => x.HasValue && x.Exists(items => items.Count() == 1)))
                    };
                }
                {
                    Guid patientId = Guid.NewGuid();
                    Patient p = new Patient
                    {
                        UUID = patientId
                    };
                    yield return new object[] {
                        new[]
                        {
                            new Patient { Id = 1, UUID = patientId }
                        },
                        new [] {
                            new BloodPressure { PatientId = 2, SystolicPressure = 120, DiastolicPressure = 80, Patient = p },
                            new BloodPressure { PatientId = 2, SystolicPressure = 128, DiastolicPressure = 95, Patient = p },
                            new BloodPressure { PatientId = 2, SystolicPressure = 130, DiastolicPressure = 95, Patient = new Patient { Id = 3, } }
                        },
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 2 },
                        ((Expression<Func<Option<IEnumerable<BloodPressureInfo>>, bool>>)(x => x.HasValue && x.Exists(items => items.Count() == 2)))
                    };
                }
            }
        }

        public static IEnumerable<object> GetOneBloodPressureAsyncCases
        {
            get
            {
                yield return new object[] {
                    Enumerable.Empty<BloodPressure>(),
                    new GetOnePhysiologicalMeasureInfo { PatientId = Guid.NewGuid(), MeasureId = Guid.NewGuid() },
                    ((Expression<Func<Option<BloodPressureInfo>, bool>>)(x => !x.HasValue))
                };

                {
                    Patient p = new Patient { UUID = Guid.NewGuid() };
                    Guid measureId = Guid.NewGuid();
                    yield return new object[] {
                        new [] {
                            new BloodPressure { Id = 10, PatientId = 1, SystolicPressure = 120, DiastolicPressure = 90, Patient = p, UUID = measureId },
                            new BloodPressure { Id = 20, PatientId = 2, SystolicPressure = 120, DiastolicPressure = 80 }
                        },
                        new GetOnePhysiologicalMeasureInfo { PatientId = p.UUID, MeasureId = measureId },
                        ((Expression<Func<Option<BloodPressureInfo>, bool>>)(x => x.HasValue && x.Exists (bp => bp.PatientId == p.UUID && bp.Id == measureId && bp.SystolicPressure == 120 && bp.DiastolicPressure == 90)))
                    };
                }
            }
        }

        public static IEnumerable<object> GetOneBodyWeightAsyncCases
        {
            get
            {
                yield return new object[] {
                    Enumerable.Empty<BodyWeight>(),
                    new GetOnePhysiologicalMeasureInfo { PatientId = Guid.NewGuid(), MeasureId = Guid.NewGuid() },
                    ((Expression<Func<Option<BodyWeightInfo>, bool>>)(x => !x.HasValue))
                };

                {
                    Patient patient = new Patient
                    {
                        UUID = Guid.NewGuid()
                    };
                    Guid measureId = Guid.NewGuid();
                    yield return new object[] {
                        new [] {
                            new BodyWeight { Id = 10, PatientId = 1, Value = 93,  Patient = patient, UUID = measureId },
                            new BodyWeight { Id = 20, PatientId = 2, Value = 80 }
                        },
                        new GetOnePhysiologicalMeasureInfo { PatientId = patient.UUID, MeasureId = measureId },
                        ((Expression<Func<Option<BodyWeightInfo>, bool>>)(x => x.HasValue && x.Exists(bw => bw.PatientId == patient.UUID && bw.Id == measureId && bw.Value == 93)))
                    };
                }
            }
        }

        [Fact]
        public async Task AddNewBloodPressureResource_Throws_NotFoundException_If_Patient_Not_Found()
        {
            // Arrange
            CreatePhysiologicalMeasureInfo<BloodPressure> input = new CreatePhysiologicalMeasureInfo<BloodPressure>
            {
                PatientId = Guid.NewGuid(),
                Measure = new BloodPressure
                {
                    DateOfMeasure = DateTimeOffset.UtcNow,
                    SystolicPressure = 120,
                    DiastolicPressure = 80
                }
            };


            // Act
            ICommand<Guid, CreatePhysiologicalMeasureInfo<BloodPressure>, BloodPressureInfo> cmd = new AddNewPhysiologicalMeasureCommand<BloodPressure, BloodPressureInfo>(input);
            Option<BloodPressureInfo, CommandException> result = await _physiologicalMeasureService.AddNewMeasureAsync(cmd);

            // Assert
            result.HasValue.Should().BeFalse();
            result.MatchNone(exception => exception.Should()
                .BeOfType<CommandEntityNotFoundException>().Which
                .Message.Should()
                .Be($"Patient <{input.PatientId}> not found"));

            _iValidateDeleteOnePhysiologicalMeasureCommandMock.Verify();
            _loggerMock.VerifyAll();
        }


        [Fact]
        public async Task AddNewBloodPressureResource()
        {
            // Arrange

            Guid patientId = Guid.NewGuid();
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Patient>().Create(new Patient { Id = 3, UUID = patientId });

                await uow.SaveChangesAsync();
            }

            // Act
            CreatePhysiologicalMeasureInfo<BloodPressure> input = new CreatePhysiologicalMeasureInfo<BloodPressure>
            {
                PatientId = patientId,
                Measure = new BloodPressure
                {
                    DateOfMeasure = DateTimeOffset.UtcNow,
                    SystolicPressure = 120,
                    DiastolicPressure = 80
                }
            };
            Option<BloodPressureInfo, CommandException> output = await _physiologicalMeasureService.AddNewMeasureAsync(new AddNewPhysiologicalMeasureCommand<BloodPressure, BloodPressureInfo>(input));

            // Assert
            output.HasValue.Should().BeTrue();
            output.MatchSome(bp =>
            {
                bp.Should().NotBeNull();
                bp.PatientId.Should().Be(input.PatientId);
                bp.Id.Should().NotBeEmpty();
                bp.DateOfMeasure.Should().Be(input.Measure.DateOfMeasure);
                bp.SystolicPressure.Should().Be(input.Measure.SystolicPressure);
                bp.DiastolicPressure.Should().Be(input.Measure.DiastolicPressure);


                _iValidateDeleteOnePhysiologicalMeasureCommandMock.Verify();
                _loggerMock.VerifyAll();

            });

        }


        [Theory]
        [MemberData(nameof(GetMostRecentMeasuresCases))]
        public async Task GetMostRecentMeasures(IEnumerable<Patient> patientsBdd, IEnumerable<BloodPressure> measuresBdd, GetMostRecentPhysiologicalMeasuresInfo query, Expression<Func<Option<IEnumerable<BloodPressureInfo>>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"Current patients store state : { SerializeObject(patientsBdd, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
            _outputHelper.WriteLine($"Current measures store state : { SerializeObject(measuresBdd, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
            _outputHelper.WriteLine($"Query : {query}");

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Patient>().Create(patientsBdd);
                uow.Repository<BloodPressure>().Create(measuresBdd);
                await uow.SaveChangesAsync();
            }

            // Act
            Option<IEnumerable<BloodPressureInfo>> measures = await _physiologicalMeasureService.GetMostRecentMeasuresAsync<BloodPressure, BloodPressureInfo>(new WantMostRecentPhysiologicalMeasuresQuery<BloodPressureInfo>(query));

            // Assert
            measures.Should().Match(resultExpectation);
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
        [MemberData(nameof(GetOneBloodPressureAsyncCases))]
        public async Task GetOneBloodPressureAsync(IEnumerable<BloodPressure> measuresBdd, GetOnePhysiologicalMeasureInfo query, Expression<Func<Option<BloodPressureInfo>, bool>> resultExpectation)
        {
            // Arrange
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<BloodPressure>().Create(measuresBdd);

                await uow.SaveChangesAsync();
            }
            // Act
            Option<BloodPressureInfo> measure = await _physiologicalMeasureService
                .GetOneMeasureAsync<BloodPressure, BloodPressureInfo>(new WantOnePhysiologicalMeasureQuery<BloodPressureInfo>(query.PatientId, query.MeasureId));

            // Assert
            measure.Should().Match(resultExpectation);
            _loggerMock.VerifyAll();
        }


        [Theory]
        [MemberData(nameof(GetOneBodyWeightAsyncCases))]
        public async Task GetOneBodyWeightAsync(IEnumerable<BodyWeight> measuresBdd, GetOnePhysiologicalMeasureInfo query, Expression<Func<Option<BodyWeightInfo>, bool>> resultExpectation)
        {
            // Arrange
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                foreach (Patient p in measuresBdd.Where(x => x.Patient != null).Select(x => x.Patient))
                {
                    uow.Repository<Patient>().Create(p);
                }

                uow.Repository<BodyWeight>().Create(measuresBdd);
                await uow.SaveChangesAsync();
            }


            // Act
            Option<BodyWeightInfo> measure = await _physiologicalMeasureService.GetOneMeasureAsync<BodyWeight, BodyWeightInfo>(new WantOnePhysiologicalMeasureQuery<BodyWeightInfo>(query.PatientId, query.MeasureId));

            // Assert
            measure.Should().Match(resultExpectation);
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
            _iValidateDeleteOnePhysiologicalMeasureCommandMock.Setup(mock => mock.ValidateAsync(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());


            // Act
            await _physiologicalMeasureService.DeleteOnePhysiologicalMeasureAsync<BloodPressure>(new DeleteOnePhysiologicalMeasureCommand(new DeletePhysiologicalMeasureInfo { Id = Guid.NewGuid(), MeasureId = Guid.NewGuid() }));

            // Assert
            _iValidateDeleteOnePhysiologicalMeasureCommandMock.Verify();
            _loggerMock.VerifyAll();

        }


        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionWhenParameterIsNullCases
        {
            get
            {
                return from uow in new[] { null, Mock.Of<IUnitOfWorkFactory>() }
                       from logger in new[] { null, Mock.Of<ILogger<PhysiologicalMeasureService>>() }
                       from validator in new[] { null, Mock.Of<IValidator<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>>() }
                       from mapper in new[] { null, Mock.Of<IMapper>() }
                       where uow == null || logger == null || validator == null || mapper == null
                       select new object[] { uow, logger, validator, mapper };

            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionWhenParameterIsNullCases))]
        public void CtorThrowsArgumentNullExceptionWhenParameterIsNull(IUnitOfWorkFactory uowFactory,
            ILogger<PhysiologicalMeasureService> logger, IValidator<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>> validator, IMapper mapper)
        {

            _outputHelper.WriteLine($"{nameof(uowFactory)} == null : {uowFactory == null}");
            _outputHelper.WriteLine($"{nameof(logger)} == null : {logger == null}");
            _outputHelper.WriteLine($"{nameof(validator)} == null : {validator == null}");
            _outputHelper.WriteLine($"{nameof(mapper)} == null : {mapper == null}");

            // Act
            Action action = () => new PhysiologicalMeasureService(uowFactory, logger, validator, mapper);


            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();

        }


        public void Dispose()
        {
            _iValidateDeleteOnePhysiologicalMeasureCommandMock = null;
            _loggerMock = null;
            _outputHelper = null;
            _unitOfWorkFactory = null;
            _physiologicalMeasureService = null;
        }
    }
}

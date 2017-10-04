
namespace MedEasy.Validators.Tests.Appointment.DTO
{
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DTO;
    using MedEasy.Validators.Appointment;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using static FluentValidation.Severity;
    using static Moq.MockBehavior;
    using static Moq.Times;
    using static Newtonsoft.Json.JsonConvert;
    using static FluentValidation.CascadeMode;

    /// <summary>
    /// Unit tests for <see cref="AppointementInfoValidator"/>
    /// </summary>
    public class CreateAppointmentInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private CreateAppointmentInfoValidator _validator;
        private Mock<IUnitOfWorkFactory> _uowFactoryMock;

        public CreateAppointmentInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _uowFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _uowFactoryMock.Setup(mock => mock.New().Dispose());

            _validator = new CreateAppointmentInfoValidator(_uowFactoryMock.Object);

        }

        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
            _uowFactoryMock = null;
        }

        [Fact]
        public void Should_Implements_AbstractValidator()
        {
            // Assert
            _validator.Should()
                .BeAssignableTo<AbstractValidator<CreateAppointmentInfo>>();
        }


        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Arguments_Null()
        {
            // Act
            Action action = () => new CreateAppointmentInfoValidator(null);

            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> ValidateCases
        {
            get
            {
                yield return new object[]
                {
                    new CreateAppointmentInfo(),
                    ((Expression<Func<ValidationResult, bool>>)(vr =>
                        !vr.IsValid &&
                        vr.Errors.Count == 4 &&
                        vr.Errors.Once(error => error.PropertyName == nameof(CreateAppointmentInfo.PatientId) && error.Severity == Error) &&
                        vr.Errors.Once(error => error.PropertyName == nameof(CreateAppointmentInfo.DoctorId) && error.Severity == Error) &&
                        vr.Errors.Once(error => error.PropertyName == nameof(CreateAppointmentInfo.StartDate) && error.Severity == Error) &&
                        vr.Errors.Once(error => error.PropertyName == nameof(CreateAppointmentInfo.Duration) && error.Severity == Error)
                    )),
                    $"No data"
                };

                yield return new object[]
                {
                    new CreateAppointmentInfo
                    {
                        DoctorId = default(Guid),
                        Duration = default(double),
                        PatientId = default(Guid),
                        StartDate = default(DateTimeOffset)
                    },
                    ((Expression<Func<ValidationResult, bool>>)(vr =>
                        !vr.IsValid &&
                        vr.Errors.Count == 4 &&
                        vr.Errors.Once(error => error.PropertyName == nameof(CreateAppointmentInfo.PatientId) && error.Severity == Error) &&
                        vr.Errors.Once(error => error.PropertyName == nameof(CreateAppointmentInfo.DoctorId) && error.Severity == Error) &&
                        vr.Errors.Once(error => error.PropertyName == nameof(CreateAppointmentInfo.StartDate) && error.Severity == Error) &&
                        vr.Errors.Once(error => error.PropertyName == nameof(CreateAppointmentInfo.Duration) && error.Severity == Error)
                    )),
                    $"All {nameof(CreateAppointmentInfo)} properties set to their default values"
                };
            }
        }

        /// <summary>
        /// Tests <see cref="CreateAppointmentInfoValidator"/> with invalid <see cref="CreateAppointmentInfo"/> instances.
        /// </summary>
        /// <param name="info">"invalid" instance to test</param>
        /// <param name="expectation"><see cref="ValidationResult"/></param>
        /// <param name="because"></param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(ValidateCases))]
        public async Task Validate(CreateAppointmentInfo info, Expression<Func<ValidationResult, bool>> expectation, string because)
        {
            // Arrange
            _outputHelper.WriteLine($"{nameof(info)} : {info}");

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);

            // Assert
            vr.Should().Match(expectation, because);
        }

        [Fact]
        public async Task Should_Fail_When_Creating_Appointment_With_Unknown_DoctorId()
        {
            // Arrange
            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Doctor>().AnyAsync(It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(false))
                .Verifiable();

            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true))
                .Verifiable();

            CreateAppointmentInfo info = new CreateAppointmentInfo
            {
                Duration = 15,
                DoctorId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                StartDate = new DateTimeOffset(2012, 12, 25, 14, 0, 0, TimeSpan.FromHours(2))
            };

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);


            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == nameof(CreateAppointmentInfo.DoctorId) && x.Severity == Error);

            _uowFactoryMock.Verify(mock => mock.New().Repository<Objects.Doctor>().AnyAsync(It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<CancellationToken>()), AtLeastOnce);
            _uowFactoryMock.Verify(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()), AtLeastOnce);
            _uowFactoryMock.Verify(mock => mock.New().Dispose(), AtLeast(2), $"a {nameof(IUnitOfWork)} instance is not dispose properly. {nameof(IUnitOfWork)}.{nameof(IUnitOfWork.Dispose)}() must be called or declare in a using block.");

        }

        [Fact]
        public async Task Should_Fail_When_Creating_Appointment_With_Unknown_PatientId()
        {
            // Arrange
            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(false))
                .Verifiable();

            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Doctor>().AnyAsync(It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true))
                .Verifiable();

            CreateAppointmentInfo info = new CreateAppointmentInfo
            {
                Duration = 15,
                DoctorId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                StartDate = new DateTimeOffset(2012, 12, 25, 14, 0, 0, TimeSpan.FromHours(2))
            };

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);

            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == nameof(CreateAppointmentInfo.PatientId) && x.Severity == Error && x.ErrorMessage == $"{nameof(Objects.Patient)} <{info.PatientId}> not found.");

            _uowFactoryMock.Verify(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()), Exactly(2));
            _uowFactoryMock.Verify(mock => mock.New().Repository<Objects.Doctor>().AnyAsync(It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<CancellationToken>()), Exactly(2));
            _uowFactoryMock.Verify(mock => mock.New().Dispose(), AtLeast(4));
        }

        [Fact]
        public void StopOnFirstFailureForEachRule() => _validator.CascadeMode.Should().Be(StopOnFirstFailure);


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(double.MinValue)]
        public async Task Should_Fail_When_Creating_Appointment_With_Bad_Duration(double duration)
        {
            _outputHelper.WriteLine($"{nameof(duration)} : {duration} min.");

            // Arrange
            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true))
                .Verifiable();

            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Doctor>().AnyAsync(It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true))
                .Verifiable();

            CreateAppointmentInfo info = new CreateAppointmentInfo
            {
                Duration = duration,
                DoctorId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                StartDate = new DateTimeOffset(2012, 12, 25, 14, 0, 0, TimeSpan.FromHours(2))
            };

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);


            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == nameof(CreateAppointmentInfo.Duration) && x.Severity == Error && x.ErrorCode == $"Err{nameof(Objects.Appointment)}Bad{nameof(CreateAppointmentInfo.Duration)}");

            _uowFactoryMock.Verify(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()), Once);
            _uowFactoryMock.Verify(mock => mock.New().Repository<Objects.Doctor>().AnyAsync(It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<CancellationToken>()), Once);
                

        }




        public static IEnumerable<object[]> ShouldSucceedCases
        {
            get
            {
                Guid doctorId = Guid.NewGuid();
                Objects.Doctor doctor = new Objects.Doctor { Id = 1, UUID = doctorId, Firstname = "Hugo", Lastname = "Strange" };

                yield return new object[]
                {
                    Enumerable.Empty<Objects.Appointment>(),
                    new CreateAppointmentInfo {
                        DoctorId = doctorId,
                        PatientId = Guid.NewGuid(),
                        StartDate = 1.February(2014).AddHours(14).AddMinutes(30),
                        Duration = 30,
                    },
                    $"No appointment in database"
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Objects.Appointment {
                            Doctor = new Objects.Doctor { UUID = Guid.NewGuid(), Firstname = "Harley", Lastname = "Quinn" },
                            StartDate = 1.February(2014).AddHours(14).AddMinutes(30),
                            EndDate = 1.February(2014).AddHours(15)
                        }
                    },
                    new CreateAppointmentInfo
                    {
                        DoctorId = doctorId,
                        PatientId = Guid.NewGuid(),
                        StartDate = 1.February(2014).AddHours(14).AddMinutes(30),
                        Duration = 30,
                    },
                    "There is an appointment at the exact same date and duration but on a different doctor schedule."
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Objects.Appointment {
                            Doctor = doctor,
                            StartDate = 1.February(2014).AddHours(14).AddMinutes(30),
                            EndDate = 1.February(2014).AddHours(15)
                        }
                    },
                    new CreateAppointmentInfo
                    {
                        DoctorId = doctor.UUID,
                        PatientId = Guid.NewGuid(),
                        StartDate = 1.February(2014).AddHours(15),
                        Duration = 30,
                    },
                    "The appointment to create starts at the exact minute the doctor's previous appointment ends."
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Objects.Appointment {
                            Doctor = doctor,
                            StartDate = 1.February(2014).AddHours(14).AddMinutes(30),
                            EndDate = 1.February(2014).AddHours(15)
                        }
                    },
                    new CreateAppointmentInfo
                    {
                        DoctorId = doctor.UUID,
                        PatientId = Guid.NewGuid(),
                        StartDate = 1.February(2014).AddHours(14),
                        Duration = 30,
                    },
                    "The appointment to create end at the exact minute the doctor's next appointment starts."
                };


            }
        }




        /// <summary>
        /// Tests that <see cref="CreateAppointmentInfoValidator"/>.
        /// 
        /// <see cref="Validation"/> that creating an <see cref="Objects.Appointment"/> 
        /// will overlaps any existing
        /// <see cref="Objects.Appointment"/>
        /// Here we consider that :
        /// <list type="bullet">
        /// both <see cref="CreateAppointmentInfo.DoctorId"/> and <see cref="CreateAppointmentInfo.PatientId"/> exists.
        /// </list> 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(ShouldSucceedCases))]
        public async Task SucceedWhenNoAppointmentExistsOnTheSelectedDatePeriodWithTheSelectedDoctor(IEnumerable<Objects.Appointment> appointments, CreateAppointmentInfo newAppointmentInfo, string because)
        {
            _outputHelper.WriteLine($"{nameof(appointments)} : {SerializeObject(appointments)}");
            _outputHelper.WriteLine($"{nameof(newAppointmentInfo)} : {SerializeObject(newAppointmentInfo)}");


            // Arrange
            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Doctor>().AnyAsync(It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true));

            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true));


            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Appointment>()
                .AnyAsync(It.IsAny<Expression<Func<Objects.Appointment, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns((Expression<Func<Objects.Appointment, bool>> filter, CancellationToken cancellationToken) 
                    => new ValueTask<bool>(appointments.Any(filter.Compile())));


            // Act
            ValidationResult vr = await _validator.ValidateAsync(newAppointmentInfo);

            // Assert
            vr.IsValid.Should().BeTrue(because);

           
            _uowFactoryMock.Verify(mock => mock.New().Dispose());

        }

        public static IEnumerable<object[]> OverlapingAppointmentCases
        {
            get
            {
                
                Objects.Doctor doctor = new Objects.Doctor { Id = 1, UUID = Guid.NewGuid(), Firstname = "Hugo", Lastname = "Strange" };
                              

                yield return new object[]
                {
                    new[]
                    {
                        new Objects.Appointment {
                            Doctor = doctor,
                            StartDate = 1.February(2014).AddHours(14).AddMinutes(30),
                            EndDate = 1.February(2014).AddHours(15)
                        }
                    },
                    new CreateAppointmentInfo
                    {
                        DoctorId = doctor.UUID,
                        PatientId = Guid.NewGuid(),
                        StartDate = 1.February(2014).AddHours(14).AddMinutes(45),
                        Duration = 30,
                    },
                    "the appointment to create starts when there's already an appointment ongoing for the same doctor."
                };

                yield return new object[]
                {
                    new[]
                    {
                        new Objects.Appointment {
                            Doctor = doctor,
                            StartDate = 1.February(2014).AddHours(14).AddMinutes(30),
                            EndDate = 1.February(2014).AddHours(15)
                        }
                    },
                    new CreateAppointmentInfo
                    {
                        DoctorId = doctor.UUID,
                        PatientId = Guid.NewGuid(),
                        StartDate = 1.February(2014).AddHours(14).AddMinutes(15),
                        Duration = 30,
                    },
                    "the appointment to create end after an already a registered appointment was started registered "
                };


            }
        }

        [Theory]
        [MemberData(nameof(OverlapingAppointmentCases))]
        public async Task FailsWhenOverlapingAppointementForTheSameDoctor(IEnumerable<Objects.Appointment> appointments, CreateAppointmentInfo newAppointmentInfo, string because)
        {
            _outputHelper.WriteLine($"{nameof(appointments)} : {SerializeObject(appointments)}");
            _outputHelper.WriteLine($"{nameof(newAppointmentInfo)} : {SerializeObject(newAppointmentInfo)}");


            // Arrange
            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Doctor>().AnyAsync(It.IsAny<Expression<Func<Objects.Doctor, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true));

            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().AnyAsync(It.IsAny<Expression<Func<Objects.Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true));


            _uowFactoryMock.Setup(mock => mock.New().Repository<Objects.Appointment>()
                .AnyAsync(It.IsAny<Expression<Func<Objects.Appointment, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns((Expression<Func<Objects.Appointment, bool>> filter, CancellationToken cancellationToken)
                    => new ValueTask<bool>(appointments.Any(filter.Compile())));


            // Act
            ValidationResult vr = await _validator.ValidateAsync(newAppointmentInfo);

            // Assert
            vr.IsValid.Should().BeFalse(because);
            vr.Errors.Should()
                .ContainSingle(x => x.PropertyName == nameof(CreateAppointmentInfo.Duration) && x.Severity == Warning, because);

            _uowFactoryMock.Verify(mock => mock.New().Dispose());

        }



    }
}

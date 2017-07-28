using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Objects;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FluentValidation.Severity;
using static Moq.MockBehavior;
using static Moq.Times;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Validators.Tests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class CreatePatientInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _uowFactoryMock;

        private IValidator<CreatePatientInfo> Validator { get; set; }

        public CreatePatientInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _uowFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _uowFactoryMock.Setup(mock => mock.New().Dispose());


            Validator = new CreatePatientInfoValidator(_uowFactoryMock.Object);

        }


        public void Dispose()
        {
            _outputHelper = null;
            Validator = null;
        }

        public static IEnumerable<object[]> ValidateTestCases
        {
            get
            {

                yield return new object[]
                {
                    new CreatePatientInfo(),
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 2
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Firstname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Lastname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because no {nameof(CreatePatientInfo)}'s data set."
                };

                yield return new object[]
                {
                    new CreatePatientInfo() { Firstname = "Bruce" },
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Lastname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because {nameof(CreatePatientInfo.Firstname)} is set and {nameof(CreatePatientInfo.Lastname)} is not"
                };

                yield return new object[]
                {
                    new CreatePatientInfo() { Lastname = "Wayne" },
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Firstname).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                    )),
                    $"because {nameof(CreatePatientInfo.Lastname)} is set and {nameof(CreatePatientInfo.Firstname)} is not"
                };

                
            }
        }

        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            // Act
            Action action = () => new CreatePatientInfoValidator(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        [Theory]
        [MemberData(nameof(ValidateTestCases))]
        public async Task ValidateTest(CreatePatientInfo info,
            Expression<Func<ValidationResult, bool>> errorMatcher,
            string because = "")
        {
            _outputHelper.WriteLine($"{nameof(info)} : {SerializeObject(info)}");

            // Act
            ValidationResult vr = await Validator.ValidateAsync(info);

            // Assert
            vr.Should()
                .Match(errorMatcher, because);
        }




        [Fact]
        public async Task Should_Fails_PatientInfo_With_Empty_Main_Doctor_Id()
        {
            // Arrange
            CreatePatientInfo patientInfo = new CreatePatientInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne",
                MainDoctorId = Guid.Empty
            };
            
            // Act
            ValidationResult vr = await Validator.ValidateAsync(patientInfo);

            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == nameof(CreatePatientInfo.MainDoctorId));

            _uowFactoryMock.Verify(mock => mock.New().Repository<Doctor>().AnyAsync(It.IsAny<Expression<Func<Doctor, bool>>>(), It.IsAny<CancellationToken>()), Never);
        }

        [Fact]
        public async Task Should_Failed_When_PatientInfo_With_Unknown_Main_Doctor_Id()
        {
            // Arrange
            _uowFactoryMock.Setup(mock =>
                        mock.New().Repository<Doctor>()
                        .AnyAsync(It.IsAny<Expression<Func<Doctor, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(false));


            CreatePatientInfo patientInfo = new CreatePatientInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne",
                MainDoctorId = Guid.NewGuid()
            };

            // Act
            ValidationResult vr = await Validator.ValidateAsync(patientInfo);

            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == nameof(CreatePatientInfo.MainDoctorId));

            _uowFactoryMock.Verify(mock => mock.New().Repository<Doctor>().AnyAsync(It.IsAny<Expression<Func<Doctor, bool>>>(), It.IsAny<CancellationToken>()), Once);

        }

    }
}

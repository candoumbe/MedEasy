
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Patients.Validators.Features.Patients.DTO;
using MedEasy.DAL.Interfaces;
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
using Patients.DTO;
using Patients.Objects;
using Xunit.Categories;
using Patients.Ids;

namespace Patients.Validators.Tests.Features.Patients
{
    /// <summary>
    /// Unit tests for <see cref="CreatePatientInfoValidator"/> class.
    /// </summary>
    [UnitTest]
    [Feature("Patients")]
    [Feature("Validation")]
    public class CreatePatientInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private CreatePatientInfoValidator _validator;
        private Mock<IUnitOfWorkFactory> _uowFactoryMock;

        public CreatePatientInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _uowFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _uowFactoryMock.Setup(mock => mock.NewUnitOfWork().Dispose());

            _validator = new CreatePatientInfoValidator(_uowFactoryMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
            _uowFactoryMock = null;
        }

        [Fact]
        public void Should_Implements_AbstractValidator() => _validator.Should()
                .BeAssignableTo<AbstractValidator<CreatePatientInfo>>();

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Arguments_Null()
        {
            // Act
            Action action = () => new CreatePatientInfoValidator(null);

            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> ValidateTestCases
        {
            get
            {
                yield return new object[]
                {
                    new CreatePatientInfo(),
                    (Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Lastname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    ),
                    $"because no {nameof(CreatePatientInfo)}'s data set."
                };

                yield return new object[]
                {
                    new CreatePatientInfo() { Firstname = "Bruce" },
                    (Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Lastname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    ),
                    $"because {nameof(CreatePatientInfo.Firstname)} is set and {nameof(CreatePatientInfo.Lastname)} is not"
                };

                yield return new object[]
                {
                    new CreatePatientInfo() { Lastname = "Wayne" },
                    (Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Firstname).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                    ),
                    $"because {nameof(CreatePatientInfo.Lastname)} is set and {nameof(CreatePatientInfo.Firstname)} is not"
                };

                yield return new object[]
                {
                    new CreatePatientInfo() { Lastname = "Wayne" },
                    (Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Firstname).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                    ),
                    $"because {nameof(CreatePatientInfo.Lastname)} is set and {nameof(CreatePatientInfo.Firstname)} is not"
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidateTestCases))]
        public async Task ValidateTest(CreatePatientInfo info,
            Expression<Func<ValidationResult, bool>> errorMatcher,
            string because = "")
        {
            _outputHelper.WriteLine($"{nameof(info)} : {SerializeObject(info)}");

            // Arrange
            _uowFactoryMock.Setup(mock => mock.NewUnitOfWork().Repository<Patient>().AnyAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(false));

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);

            // Assert
            vr.Should()
                .Match(errorMatcher, because);
        }

        [Fact]
        public async Task Should_Fails_When_Id_AlreadyExists()
        {
            // Arrange
            _uowFactoryMock.Setup(mock =>
                        mock.NewUnitOfWork().Repository<Patient>()
                        .AnyAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true));

            CreatePatientInfo info = new()
            {
                Firstname = "Bruce",
                Lastname = "Wayne",
                Id = PatientId.New()
            };

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);

            // Assert
            vr.IsValid.Should().BeFalse($"{nameof(Patient)} <{info.Id}> already exists");
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == nameof(CreatePatientInfo.Id));

            _uowFactoryMock.Verify(mock => mock.NewUnitOfWork().Repository<Patient>().AnyAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<CancellationToken>()), Once);
        }

    }
}

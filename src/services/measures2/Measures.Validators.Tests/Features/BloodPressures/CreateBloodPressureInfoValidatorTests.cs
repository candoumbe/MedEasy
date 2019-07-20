using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Measures.DTO;
using Measures.Objects;
using MedEasy.DAL.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Newtonsoft.Json.JsonConvert;
using static FluentValidation.Severity;
using static Moq.MockBehavior;
using System.Linq;
using Measures.Validators.Commands.BloodPressures;
using Xunit.Categories;

namespace Measures.Validators.Tests.Features.BloodPressures
{
    [UnitTest]
    [Feature("Validation")]
    public class CreateBloodPressureInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        private CreateBloodPressureInfoValidator _validator;
        private Mock<IUnitOfWorkFactory> _uowFactoryMock;

        public CreateBloodPressureInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _uowFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _uowFactoryMock.Setup(mock => mock.NewUnitOfWork().Dispose());

            _validator = new CreateBloodPressureInfoValidator(_uowFactoryMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _uowFactoryMock = null;
            _validator = null;
        }

        [Fact]
        public void Should_Implements_AbstractValidator() => _validator.Should()
                .BeAssignableTo<AbstractValidator<CreateBloodPressureInfo>>();

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Arguments_Null()
        {
            // Act
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new CreateBloodPressureInfoValidator(null);
#pragma warning restore IDE0039 // Utiliser une fonction locale

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
                    new CreateBloodPressureInfo(),
                    Enumerable.Empty<Patient>(),
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 3
                            && vr.Errors.Once(errorItem => nameof(CreateBloodPressureInfo.PatientId).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                            && vr.Errors.Once(errorItem => nameof(CreateBloodPressureInfo.SystolicPressure).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                            && vr.Errors.Once(errorItem => nameof(CreateBloodPressureInfo.DiastolicPressure).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                    )),
                    $"because no {nameof(CreateBloodPressureInfo)}'s data set."
                };

                yield return new object[]
                {
                    new CreateBloodPressureInfo {
                        SystolicPressure = 80, DiastolicPressure = 120,
                        PatientId = Guid.NewGuid()
                    } ,
                    Enumerable.Empty<Patient>(),
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 2
                            && vr.Errors.Once(errorItem => nameof(CreateBloodPressureInfo.DiastolicPressure).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                            && vr.Errors.Once(errorItem => nameof(CreateBloodPressureInfo.PatientId).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because {nameof(CreateBloodPressureInfo.SystolicPressure)} < {nameof(CreateBloodPressureInfo.DiastolicPressure)} " +
                    $"and {nameof(CreateBloodPressureInfo.PatientId)} does not exist."
                };

                yield return new object[]
                {
                    new CreateBloodPressureInfo {
                        SystolicPressure = 120, DiastolicPressure = 80,
                        PatientId = Guid.NewGuid()
                    },
                    Enumerable.Empty<Patient>(),
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => $"{nameof(CreateBloodPressureInfo.PatientId)}".Equals(errorItem.PropertyName) && errorItem.Severity == Error )
                    )),
                    $"{nameof(CreateBloodPressureInfo.PatientId)} does not exist."

                };

                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new CreateBloodPressureInfo {
                            SystolicPressure = 120, DiastolicPressure = 80,
                            PatientId = patientId
                        },
                        new []{
                            new Patient(patientId).ChangeNameTo("Freeze")
                        },
                        ((Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid)),
                        $"because both {nameof(CreateBloodPressureInfo)}.{nameof(CreateBloodPressureInfo.PatientId)} exists and " +
                        $"other {nameof(CreateBloodPressureInfo)}.{nameof(CreateBloodPressureInfo.PatientId)}'s properties " +
                        "cannot be set at the same time."

                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ValidateTestCases))]
        public async Task ValidateTest(CreateBloodPressureInfo info, IEnumerable<Patient> patients,
            Expression<Func<ValidationResult, bool>> errorMatcher,
            string because = "")
        {
            _outputHelper.WriteLine($"{nameof(info)} : {SerializeObject(info)}");

            // Arrange
            _uowFactoryMock.Setup(mock =>  mock.NewUnitOfWork().Repository<Patient>().AnyAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(patients.Any(x => x.Id == info.PatientId)));

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);

            // Assert
            vr.Should()
                .Match(errorMatcher, because);
        }
    }
}

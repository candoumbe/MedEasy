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

namespace Measures.Validators
{
    public class CreateBloodPressureInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        private CreateBloodPressureInfoValidator _validator;
        private Mock<IUnitOfWorkFactory> _uowFactoryMock;

        public CreateBloodPressureInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _uowFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _uowFactoryMock.Setup(mock => mock.New().Dispose());

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
            Action action = () => new CreateBloodPressureInfoValidator(null);

            action.ShouldThrow<ArgumentNullException>().Which
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
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 2
                            && vr.Errors.Once(errorItem => nameof(CreateBloodPressureInfo.SystolicPressure).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                            && vr.Errors.Once(errorItem => nameof(CreateBloodPressureInfo.DiastolicPressure).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                    )),
                    $"because no {nameof(CreateBloodPressureInfo)}'s data set."
                };

                yield return new object[]
                {
                    new CreateBloodPressureInfo { SystolicPressure = 80, DiastolicPressure = 120 } ,
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreateBloodPressureInfo.DiastolicPressure).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because no {nameof(CreateBloodPressureInfo)}'s data set."
                };

            }
        }


        [Theory]
        [MemberData(nameof(ValidateTestCases))]
        public async Task ValidateTest(CreateBloodPressureInfo info,
            Expression<Func<ValidationResult, bool>> errorMatcher,
            string because = "")
        {
            _outputHelper.WriteLine($"{nameof(info)} : {SerializeObject(info)}");

            // Arrange
            _uowFactoryMock.Setup(mock => mock.New().Repository<Patient>().AnyAsync(It.IsAny<Expression<Func<Patient, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(false));

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);

            // Assert
            vr.Should()
                .Match(errorMatcher, because);
        }

    }
}

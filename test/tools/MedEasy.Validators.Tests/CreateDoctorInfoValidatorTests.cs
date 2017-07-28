using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using MedEasy.DTO;
using Xunit;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Xunit.Abstractions;
using Moq;
using static FluentValidation.Severity;
using static Newtonsoft.Json.JsonConvert;
using MedEasy.DAL.Interfaces;
using static Moq.MockBehavior;
using MedEasy.Objects;
using System.Threading;
using static Moq.Times;

namespace MedEasy.Validators.Tests
{

    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class CreateDoctorInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _uowFactoryMock;

        private IValidator<CreateDoctorInfo> Validator { get; set; }

        public CreateDoctorInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _uowFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _uowFactoryMock.Setup(mock => mock.New().Dispose());

            Validator = new CreateDoctorInfoValidator(_uowFactoryMock.Object);

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
                    new CreateDoctorInfo(),
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreateDoctorInfo.Lastname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because no {nameof(CreateDoctorInfo)}'s data set."
                };

                yield return new object[]
                {
                    new CreateDoctorInfo() { Firstname = "Bruce" },
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreateDoctorInfo.Lastname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because {nameof(CreateDoctorInfo.Firstname)} is set and {nameof(CreateDoctorInfo.Lastname)} is not"
                };

                yield return new object[]
                {
                    new CreateDoctorInfo() { Lastname = "Wayne" },
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreateDoctorInfo.Firstname).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                    )),
                    $"because {nameof(CreateDoctorInfo.Lastname)} is set and {nameof(CreateDoctorInfo.Firstname)} is not"
                };


            }
        }

        [Theory]
        [MemberData(nameof(ValidateTestCases))]
        public async Task ValidateTest(CreateDoctorInfo info,
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
        public async Task Should_Fails_When_SpecialtyId_Is_Empty()
        {
            // Arrange
            CreateDoctorInfo info = new CreateDoctorInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne",
                SpecialtyId = Guid.Empty
            };

            // Act
            ValidationResult vr = await Validator.ValidateAsync(info);

            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == nameof(CreateDoctorInfo.SpecialtyId));

            _uowFactoryMock.Verify(mock => mock.New().Repository<Specialty>().AnyAsync(It.IsAny<Expression<Func<Specialty, bool>>>(), It.IsAny<CancellationToken>()), Never);
        }

        [Fact]
        public async Task Should_Fails_When_SpecialtyId_Is_Unknown()
        {
            // Arrange
            _uowFactoryMock.Setup(mock =>
                        mock.New().Repository<Specialty>()
                        .AnyAsync(It.IsAny<Expression<Func<Specialty, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(false));


            CreateDoctorInfo info = new CreateDoctorInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne",
                SpecialtyId = Guid.NewGuid()
            };

            // Act
            ValidationResult vr = await Validator.ValidateAsync(info);

            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == nameof(CreateDoctorInfo.SpecialtyId));

            _uowFactoryMock.Verify(mock => mock.New().Repository<Specialty>().AnyAsync(It.IsAny<Expression<Func<Specialty, bool>>>(), It.IsAny<CancellationToken>()), Once);

        }

    }
}

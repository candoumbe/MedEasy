using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using Xunit;
using System.Linq;
using Xunit.Abstractions;
using MedEasy.Validators.Patient;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentValidation;
using Moq;
using static FluentValidation.Severity;
using static Moq.MockBehavior;
using System.Threading;

namespace MedEasy.Validators.Tests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class CreatePatientCommandValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IValidator<CreatePatientInfo>> _createPatientInfoValidatorMock;
        private CreatePatientCommandValidator _validator;

        public CreatePatientCommandValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _createPatientInfoValidatorMock = new Mock<IValidator<CreatePatientInfo>>(Strict);

            _validator = new CreatePatientCommandValidator(_createPatientInfoValidatorMock.Object);
            
        }


        /// <summary>
        /// Tests that 
        /// <code>
        ///     new <see cref="CreatePatientCommand /> new <see cref="CreatePatientInfo"/>();
        /// </code>
        /// validation fails when the <see cref="CreatePatientInfo"/> validation fails. 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_Fail_When_Data_Validation_Fails()
        {
            // Arrange
            _createPatientInfoValidatorMock.Setup(mock => mock.ValidateAsync(It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(
                    new[]
                    {
                        new ValidationFailure(nameof(CreatePatientInfo.Firstname), $"{nameof(CreatePatientInfo.Firstname)} cannot be null") { Severity = Error, },
                        new ValidationFailure(nameof(CreatePatientInfo.Lastname), $"{nameof(CreatePatientInfo.Lastname)} cannot be null") { Severity = Error}
                    })
                );

            // Act
            ICreatePatientCommand cmd = new CreatePatientCommand(new CreatePatientInfo());
            ValidationResult vr = await _validator.ValidateAsync(cmd);

            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(2).And
                .Contain(error => error.PropertyName == nameof(cmd.Data.Firstname) && error.Severity == Error).And
                .Contain(error => error.PropertyName == nameof(cmd.Data.Lastname) && error.Severity == Error);


        }

        
        public void Dispose()
        {
            _outputHelper = null;
            _createPatientInfoValidatorMock = null;
            _validator = null;
        }


    }
}

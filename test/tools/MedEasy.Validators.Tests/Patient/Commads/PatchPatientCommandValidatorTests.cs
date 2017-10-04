﻿using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MedEasy.Commands;
using MedEasy.DTO;
using MedEasy.Validators.Patient.Commands;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FluentValidation.Severity;
using static Moq.MockBehavior;

namespace MedEasy.Validators.Tests.Patient.Commands
{
    public class ValidatePatchPatientCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private PatchPatientCommandValidator _validator;
        private Mock<IValidator<PatchInfo<Guid, PatientInfo>>> _patchPatientInfoValidatorMock;

        public ValidatePatchPatientCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _patchPatientInfoValidatorMock = new Mock<IValidator<PatchInfo<Guid, PatientInfo>>>(Strict);

            _validator = new PatchPatientCommandValidator(_patchPatientInfoValidatorMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
            _patchPatientInfoValidatorMock = null;
        }
        
        [Fact]
        public async Task Should_Fails_When_Data_Validation_Fails()
        {
            // Arrange
            _patchPatientInfoValidatorMock.Setup(mock => mock.ValidateAsync(It.IsAny<ValidationContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] {
                    new ValidationFailure("PropName", "a description")
                } ))
                .Verifiable();
            
            IPatchCommand<Guid, PatientInfo> command = new PatchCommand<Guid, PatientInfo>(new PatchInfo<Guid, PatientInfo>());
            _outputHelper.WriteLine($"Command to validate : {command} ");

            // Act
            ValidationResult vr = await _validator.ValidateAsync(command);

            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == "PropName");

            _patchPatientInfoValidatorMock.Verify();

        }

    }
}

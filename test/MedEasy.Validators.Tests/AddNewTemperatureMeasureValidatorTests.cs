﻿using FluentAssertions;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.Validators.ErrorLevel;


namespace MedEasy.Validators.Tests
{
    public class AddNewTemperatureMeasureValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private AddNewTemperatureMeasureCommandValidator _validator;

        public AddNewTemperatureMeasureValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _validator = new AddNewTemperatureMeasureCommandValidator();
        }


        public static IEnumerable<object> CommandsNotValidCases
        {
            get
            {
                yield return new object[]
                {
                    new AddNewTemperatureCommand(new CreateTemperatureInfo {PatientId = int.MinValue,Value = int.MinValue, Timestamp = DateTime.Now }),
                    $"because int.MinValue is not valid for {nameof(CreateTemperatureInfo.PatientId)}"
                };

                yield return new object[]
                {
                    new AddNewTemperatureCommand(new CreateTemperatureInfo {PatientId = 0,Value = int.MinValue, Timestamp = DateTime.Now }),
                    $"because 0 is not valid for {nameof(CreateTemperatureInfo.PatientId)}"
                };
                yield return new object[]
                {
                    new AddNewTemperatureCommand(new CreateTemperatureInfo {PatientId = -1,Value = int.MinValue, Timestamp = DateTime.Now }),
                    $"because -1 is not valid for {nameof(CreateTemperatureInfo.PatientId)}"
                };
            }
        }


        [Theory]
        [MemberData(nameof(CommandsNotValidCases))]
        public async Task ValidateShouldReturnsErrors(IAddNewTemperatureMeasureCommand cmd, string reason)
        {
            _outputHelper.WriteLine($"Validation of {cmd}");
            IEnumerable<Task<ErrorInfo>> validationsResults = _validator.Validate(cmd);

            IEnumerable<ErrorInfo> errors = await Task.WhenAll(validationsResults).ConfigureAwait(false);
            errors.Should()
                .NotBeNull();

            errors.Any(x => x.Severity == Error).Should().BeTrue();
        }


        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
        }
    }
}
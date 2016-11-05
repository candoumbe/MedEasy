using FluentAssertions;
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


        public static IEnumerable<object> CommandsValidCases
        {
            get
            {
                yield return new object[]
                {
                    new CreateTemperatureInfo {Value = int.MinValue, DateOfMeasure = DateTimeOffset.UtcNow },
                    $"because int.MinValue is valid for {nameof(CreateTemperatureInfo.Value)}"
                };

                yield return new object[]
                {
                    new CreateTemperatureInfo {Value = int.MinValue, DateOfMeasure = DateTimeOffset.UtcNow },
                    $"because 0 is valid for {nameof(CreateTemperatureInfo.Value)}"
                };
                yield return new object[]
                {
                    new CreateTemperatureInfo {Value = int.MinValue, DateOfMeasure = DateTimeOffset.UtcNow },
                    $"because int.MaxValue valid for {nameof(CreateTemperatureInfo.Value)}"
                };
            }
        }


        [Theory]
        [MemberData(nameof(CommandsValidCases))]
        public async Task ValidateShouldReturnsErrors(CreateTemperatureInfo input, string reason)
        {
            _outputHelper.WriteLine($"Validation of {input}");
            IEnumerable<Task<ErrorInfo>> validationsResults = _validator.Validate(input);

            IEnumerable<ErrorInfo> errors = await Task.WhenAll(validationsResults).ConfigureAwait(false);
            errors.Should().BeEmpty();
            
        }


        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
        }
    }
}

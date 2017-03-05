using FluentAssertions;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Objects;
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
                    new CreatePhysiologicalMeasureInfo<Temperature> {
                        Measure = new Temperature {Value = int.MinValue, DateOfMeasure = DateTimeOffset.UtcNow }
                    },
                    $"because {nameof(Temperature.Value)} == int.MinValue"
                };

                yield return new object[]
                {
                    new CreatePhysiologicalMeasureInfo<Temperature> {
                        Measure = new Temperature {Value = 0, DateOfMeasure = DateTimeOffset.UtcNow }
                    },
                    $"because {nameof(Temperature.Value)} == 0"
                };
                yield return new object[]
                {
                    new CreatePhysiologicalMeasureInfo<Temperature> {
                        Measure = new Temperature {Value = int.MaxValue, DateOfMeasure = DateTimeOffset.UtcNow }
                    },
                    $"because {nameof(Temperature.Value)} == int.MinValue"
                };
            }
        }


        [Theory]
        [MemberData(nameof(CommandsValidCases))]
        public async Task ValidateShouldReturnsErrors(CreatePhysiologicalMeasureInfo<Temperature> input, string reason)
        {
            _outputHelper.WriteLine($"Validation of {input}");


            IEnumerable<Task<ErrorInfo>> validationsResults = _validator.Validate(input);

            IEnumerable<ErrorInfo> errors = await Task.WhenAll(validationsResults);
            errors.Should().BeEmpty();
            
        }


        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
        }
    }
}

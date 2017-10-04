using FluentAssertions;
using FluentValidation.Results;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Objects;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FluentValidation.Severity;



namespace MedEasy.Validators.Tests
{
    public class AddNewTemperatureMeasureValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private AddNewPhysiologicalMeasureCommandValidator<Temperature> _validator;

        public AddNewTemperatureMeasureValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _validator = new AddNewTemperatureMeasureCommandValidator();
        }

        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
        }

        public static IEnumerable<object> CommandsInvalidCases
        {
            get
            {
                yield return new object[]
                {
                    new CreatePhysiologicalMeasureInfo<Temperature> {
                        PatientId = Guid.Empty,
                        Measure = new Temperature {Value = int.MinValue, DateOfMeasure = DateTimeOffset.UtcNow }
                    },
                    ((Expression<Func<ValidationResult, bool>>)(vr =>
                        !vr.IsValid &&
                        vr.Errors.Count == 1 &&
                        vr.Errors[0].PropertyName == nameof(CreatePhysiologicalMeasureInfo<Temperature>.PatientId) && vr.Errors[0].Severity == Error
                    )),
                    $"{nameof(CreatePhysiologicalMeasureInfo<Temperature>.PatientId)} == Guid.Empty"
                };

                
                yield return new object[]
                {
                    new CreatePhysiologicalMeasureInfo<Temperature> {
                        PatientId =  Guid.NewGuid(),
                        Measure = new Temperature {Value = int.MaxValue, DateOfMeasure = DateTimeOffset.UtcNow }
                    },

                    ((Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid)),
                    $"because {nameof(Temperature.Value)} == int.MaxValue"
                };
            }
        }


        [Theory]
        [MemberData(nameof(CommandsInvalidCases))]
        public async Task Should_Fails(CreatePhysiologicalMeasureInfo<Temperature> input, Expression<Func<ValidationResult, bool>> expectation, string because)
        {
            _outputHelper.WriteLine($"{nameof(input)} : {input}");

            // Act
            ValidationResult vr = await _validator.ValidateAsync(input);

            // Assert
            vr.Should().Match(expectation, because);
            
            
        }

    }
}

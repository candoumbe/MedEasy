using Bogus;

using FluentAssertions;
using FluentAssertions.Extensions;

using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Measures.Objects.UnitTests
{
    [UnitTest]
    [Feature(nameof(Measures))]
    [Feature(nameof(BodyWeight))]
    public class BodyWeightTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly static Faker _faker;

        static BodyWeightTests()
        {
            _faker = new Faker();
        }

        public BodyWeightTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> CtorArgumentOutOfRangeExceptionCases
        {
            get
            {
                yield return new object[] { Guid.Empty, Guid.NewGuid(), 12.December(2013), 8, "Measure is empty" };
                yield return new object[] { Guid.NewGuid(), Guid.Empty, 12.December(2013), 8, "Patient id is empty" };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, 8, "Date of measure is DateTime.MinValue" };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), -10, "Value is less than 0" };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), float.PositiveInfinity, "value is PositiveInfinity." };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), float.NaN, "value is NaN." };
            }
        }

        [Theory]
        [MemberData(nameof(CtorArgumentOutOfRangeExceptionCases))]
        public void Ctor_throws_ArgumentOutOfRangeException_when_argument_is_out_of_expected_range(Guid id, Guid patientId, DateTime dateOfMeasure, float value, string reason)
        {
            // Arrange
            _outputHelper.WriteLine($"{nameof(patientId)} : '{patientId}'");
            _outputHelper.WriteLine($"{nameof(id)} : '{id}'");
            _outputHelper.WriteLine($"{nameof(dateOfMeasure)} : '{dateOfMeasure}'");
            _outputHelper.WriteLine($"{nameof(value)} : '{value}'");

            // Act
            Action ctorWithInvalidArgument = () => new BodyWeight(id, patientId, dateOfMeasure, value);

            // Assert
            ctorWithInvalidArgument.Should()
                                   .ThrowExactly<ArgumentOutOfRangeException>(reason);
        }

        [Fact]
        public void Ctor_can_builds_measure_with_zero()
        {
            // Act
            BodyWeight measure = new BodyWeight(Guid.NewGuid(), Guid.NewGuid(), 13.January(2009), 0);

            // Assert
            measure.Value.Should()
                         .Be(0);
        }

        public static IEnumerable<object[]> ChangeValueInvalidCases
        {
            get
            {
                BodyWeight measure = new BodyWeight(Guid.NewGuid(), Guid.NewGuid(), 13.January(2009), 8);

                yield return new object[]
                {
                    measure,
                    _faker.Random.Float(min: float.MinValue, max: 0 - float.Epsilon),
                    "new value is less than 0"
                };

                yield return new object[]
                {
                    measure,
                    float.NaN,
                    $"new value is {float.NaN}"
                };

                yield return new object[]
                {
                    measure,
                    float.PositiveInfinity,
                    $"new value is {float.PositiveInfinity}"
                };
            }
        }

        [Theory]
        [MemberData(nameof(ChangeValueInvalidCases))]
        public void ChangeValue_with_incorrect_value_throws_ArgumentOutOfRangeException(BodyWeight measure, float newValue, string reason)
        {
            // Act
            Action changeValue = () => measure.ChangeValueTo(newValue);

            // Assert
            changeValue.Should()
                       .ThrowExactly<ArgumentOutOfRangeException>(reason)
                       .Where(ex => ex.ParamName != null)
                       .Where(ex => ex.ActualValue != null, "the value that causes the exception to be thrown should be advertised ")
                       .Where(ex => !string.IsNullOrWhiteSpace(ex.Message));
        }

        [Fact]
        public void ChangeValue_sets_Value()
        {
            // Arrange
            BodyWeight measure = new BodyWeight(Guid.NewGuid(), Guid.NewGuid(), 13.January(2009), 0);
            float newValue = _faker.Random.Float(min: 0, max: float.MaxValue);

            // Act
            measure.ChangeValueTo(newValue);

            // Assert
            measure.Value.Should()
                         .Be(newValue);
        }
    }
}

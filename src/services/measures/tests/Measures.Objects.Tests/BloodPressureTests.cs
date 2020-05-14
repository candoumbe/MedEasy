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
    [Feature(nameof(BloodPressure))]
    public class BloodPressureTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly static Faker _faker;

        static BloodPressureTests()
        {
            _faker = new Faker();
        }

        public BloodPressureTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> CtorArgumentOutOfRangeExceptionCases
        {
            get
            {
                yield return new object[] { Guid.Empty, Guid.NewGuid(), 12.December(2013), 13.5, 8, "PatientId is empty" };
                yield return new object[] { Guid.NewGuid(), Guid.Empty, 12.December(2013), 13.5, 8, "Measure id is empty" };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, 13.5, 8, "Date of measure is DateTime.MinValue" };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), -10, 8, "diastolic is less than 0" };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), 13.5, -8, "systolic is less than 0" };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), float.PositiveInfinity, 8, "systolic is PositiveInfinity." };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), float.NaN, 8, "systolic is NaN." };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), 13.5f, float.NaN, "diastolic is NaN." };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), 5, 6, "systolic is lower than diastolic." };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), 14.January(1985), float.MaxValue, float.PositiveInfinity, "diastolic is PositiveInfity." };
            }
        }

        [Theory]
        [MemberData(nameof(CtorArgumentOutOfRangeExceptionCases))]
        public void Ctor_throws_ArgumentOutOfRangeException_when_argument_is_out_of_expected_range(Guid patientId, Guid id, DateTime dateOfMeasure, float systolic, float diastolic, string reason)
        {
            // Arrange
            _outputHelper.WriteLine($"{nameof(patientId)} : '{patientId}'");
            _outputHelper.WriteLine($"{nameof(id)} : '{id}'");
            _outputHelper.WriteLine($"{nameof(dateOfMeasure)} : '{dateOfMeasure}'");
            _outputHelper.WriteLine($"{nameof(diastolic)} : '{diastolic}'");
            _outputHelper.WriteLine($"{nameof(systolic)} : '{systolic}'");

            // Act
            Action ctorWithInvalidArgument = () => new BloodPressure(patientId, id, dateOfMeasure, systolic, diastolic);

            // Assert
            ctorWithInvalidArgument.Should()
                                   .ThrowExactly<ArgumentOutOfRangeException>(reason);
        }

        public static IEnumerable<object[]> ChangeSystolicPressureInvalidValueCases
        {
            get
            {
                BloodPressure measure = new BloodPressure(Guid.NewGuid(), Guid.NewGuid(), 13.January(2009), systolicPressure : 13.5f, diastolicPressure: 8);

                yield return new object[]
                {
                    measure,
                    _faker.Random.Float(min: float.MinValue, max: 0 - float.Epsilon),
                    "cannot change systolic value to value less than 0"
                };

                yield return new object[]
                {
                    measure,
                    float.NaN,
                    $"cannot change systolic value to {float.NaN}"
                };

                yield return new object[]
                {
                    measure,
                    float.PositiveInfinity,
                    $"cannot change systolic value to {float.PositiveInfinity}"
                };

                yield return new object[]
                {
                    measure,
                    _faker.Random.Float(max: measure.DiastolicPressure - float.Epsilon),
                    $"cannot change systolic value to be less than {nameof(measure.DiastolicPressure)}"
                };
            }
        }

        [Theory]
        [MemberData(nameof(ChangeSystolicPressureInvalidValueCases))]
        public void ChangeSystolicPressure_with_incorrect_value_throws_ArgumentOutOfRangeException(BloodPressure measure, float newSystolicValue, string reason)
        {
            // Act
            Action changeSystolicValue = () => measure.ChangeSystolicTo(newSystolicValue);

            // Assert
            changeSystolicValue.Should()
                               .ThrowExactly<ArgumentOutOfRangeException>(reason);
        }

        [Fact]
        public void Ctor_can_builds_measure_with_zero()
        {
            // Act
            BloodPressure measure = new BloodPressure(Guid.NewGuid(), Guid.NewGuid(), 13.January(2009), 0, 0);

            // Assert
            measure.DiastolicPressure.Should()
                                     .Be(0);
            measure.SystolicPressure.Should()
                                    .Be(0);
        }

        public static IEnumerable<object[]> ChangeDiastolicPressureInvalidValueCases
        {
            get
            {
                BloodPressure measure = new BloodPressure(Guid.NewGuid(), Guid.NewGuid(), 13.January(2009), systolicPressure: 13.5f, diastolicPressure: 8);

                yield return new object[]
                {
                    measure,
                    _faker.Random.Float(min: float.MinValue, max: 0 - float.Epsilon),
                    "cannot change diastolic value to value less than 0"
                };

                yield return new object[]
                {
                    measure,
                    float.NaN,
                    $"cannot change diastolic value to {float.NaN}"
                };

                yield return new object[]
                {
                    measure,
                    _faker.Random.Float(min: measure.SystolicPressure + float.Epsilon, max: float.MaxValue),
                    $"cannot change diastolic to value greater than {nameof(BloodPressure.SystolicPressure)} ({measure.SystolicPressure})"
                };
            }
        }

        [Theory]
        [MemberData(nameof(ChangeDiastolicPressureInvalidValueCases))]
        public void ChangeDiastolicPressure_with_incorrect_value_throws_ArgumentOutOfRangeException(BloodPressure measure, float newDiastolicValue, string reason)
        {
            // Act
            Action changeDiastolicValue = () => measure.ChangeDiastolicTo(newDiastolicValue);

            // Assert
            changeDiastolicValue.Should()
                                .ThrowExactly<ArgumentOutOfRangeException>(reason)
                                .Where(ex => ex.ParamName != null)
                                .Where(ex => ex.ActualValue != null, "the value that causes the exception to be thrown should be advertised ")
                                .Where(ex => !string.IsNullOrWhiteSpace(ex.Message));
        }

        [Fact]
        public void ChangeSystolic_sets_Systolic_with_passed_value()
        {
            // Arrange
            BloodPressure measure = new BloodPressure(Guid.NewGuid(), Guid.NewGuid(), 13.January(2009), systolicPressure: 13.5f, diastolicPressure: 9f);
            float newValue = _faker.Random.Float(min: measure.DiastolicPressure, max: 25);

            // Act
            measure.ChangeSystolicTo(newValue);

            // Assert
            measure.SystolicPressure.Should()
                                    .Be(newValue);
        }

        [Fact]
        public void ChangeDiastolicPressure_sets_Diastolic_with_passed_value()
        {
            // Arrange
            BloodPressure measure = new BloodPressure(Guid.NewGuid(), Guid.NewGuid(), 13.January(2009), systolicPressure: 13.5f, diastolicPressure: 9f);
            float newValue = _faker.Random.Float(min: 0, max: measure.SystolicPressure);

            // Act
            measure.ChangeDiastolicTo(newValue);

            // Assert
            measure.DiastolicPressure.Should()
                                     .Be(newValue);
        }

        [Fact]
        public void ChangeDiastolicPressure_throws_InvalidOperationException_when_value_would_be_greater_than_Systolic_actual_value()
        {
            // Arrange
            float systolic = _faker.Random.Float(min: 0, max: 20);
            float diastolic = _faker.Random.Float(min: 0, max: systolic);

            BloodPressure measure = new BloodPressure(Guid.NewGuid(), Guid.NewGuid(), 13.January(2009), systolicPressure: systolic,diastolicPressure: diastolic);
            float newValue = _faker.Random.Float(min: measure.SystolicPressure + float.Epsilon, max: float.PositiveInfinity);

            _outputHelper.WriteLine($"Current measure : {measure}");

            // Act
            Action changeDiastolic = () => measure.ChangeDiastolicTo(newValue);

            // Assert
            changeDiastolic.Should()
                           .ThrowExactly<ArgumentOutOfRangeException>($"the diastolic cannot be changed to a value greater than current {measure.SystolicPressure}")
                           .Where(ex => ex.ParamName != null)
                           .Where(ex => ex.ActualValue != null, "the value that causes the exception to be thrown should be advertised ")
                           .Where(ex => ex.Message != null);
        }
    }
}

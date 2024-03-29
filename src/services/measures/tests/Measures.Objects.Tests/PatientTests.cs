namespace Measures.Objects.Tests
{
    using System;
    using Xunit;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using System.Collections.Generic;
    using Xunit.Categories;
    using Measures.Objects.Exceptions;
    using NodaTime.Extensions;
    using NodaTime;
    using Measures.Ids;

    [UnitTest]
    public class PatientTests
    {
        [Fact]
        public void Ctor_Should_Throws_ArgumentOutRangeException_When_UUID_IsEmpty()
        {
            // Act
            Action ctor = () => new Subject(SubjectId.Empty, "John Doe");

            // Assert
            ctor.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"{nameof(Subject)}.{nameof(Subject.Id)} must not be empty");
        }

        [Fact]
        public void Ctor_Should_Create_Valid_Instance()
        {
            // Arrange
            SubjectId id = SubjectId.New();
            const string initialName = "John Doe";

            // Act
            Subject expected = new(id, initialName);

            // Assert
            expected.Id.Should()
                .Be(id);
            expected.Name.Should()
                .Be(initialName);

            expected.BloodPressures.Should()
                .BeEmpty();
        }

        [Theory]
        [InlineData("J'onzz J'onzz", "J'onzz J'onzz", "The new name is set 'as is' because there no leading or trailing spaces")]
        [InlineData("clark kent", "Clark Kent", "The new name is set using proper case.")]
        [InlineData("", "", "Changing name to string.empty is allowed")]
        [InlineData("  ", "", "Changing name to string.empty is allowed")]
        [InlineData("clark kent  ", "Clark Kent", "ChangeName remove leading to string.empty is allowed")]
        public void ChangeName_Set_Name_Property(string newName, string expected, string reason)
        {
            // Arrange
            Subject patient = new(SubjectId.New(), "John Doe");

            // Act
            patient.ChangeNameTo(newName);

            // Assert
            patient.Name.Should()
                .Be(expected, reason);
        }

        [Fact]
        public void ChangingName_To_Null_Throws_ArgumentNullException()
        {
            // Arrange
            Subject patient = new(SubjectId.New(), "clark kent");

            // Act
            Action changingName = () => patient.ChangeNameTo(null);

            // Assert
            patient.Name.Should()
                .Be("Clark Kent");
            changingName.Should()
                .ThrowExactly<ArgumentNullException>($"{nameof(Subject)}.{nameof(Subject.Name)} cannot be set to null.");
        }

        public static IEnumerable<object[]> AddBloodPressureCases
        {
            get
            {
                yield return new object[] { BloodPressureId.New(), 10.April(2012).AsUtc().ToInstant(), 120, 80 };
            }
        }

        [Theory]
        [MemberData(nameof(AddBloodPressureCases))]
        public void AddingBloodPressure_Should_AddMeasure(BloodPressureId measureId, Instant dateOfMeasure, float systolic, float diastolic)
        {
            // Arrange
            Subject patient = new(SubjectId.New(), "John Doe");

            // Act
            patient.AddBloodPressure(measureId, dateOfMeasure, systolic, diastolic);

            // Assert
            BloodPressure measure = patient.BloodPressures.Should()
                .HaveCount(1, "The collection was empty before adding one measure").And
                .ContainSingle().Which.Should()
                    .BeOfType<BloodPressure>().Which;

            measure.Id.Should()
                .Be(measureId);
            measure.DateOfMeasure.Should()
                .Be(dateOfMeasure);
            measure.DiastolicPressure.Should()
                .Be(diastolic);
        }

        [Fact]
        public void AddingBloodPressureWithNoId_Throws_ArgumentOutOfRangeException()
        {
            // Arrange
            Subject patient = new(SubjectId.New(), "John Doe");

            // Act
            Action addWithNoId = () => patient.AddBloodPressure(BloodPressureId.Empty, 22.April(2014).AsUtc().ToInstant(), systolic: 150, diastolic: 80);

            // Assert
            addWithNoId.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("id of the measure must be provided and it cannot be empty");
        }

        [Fact]
        public void AddingBloodPressureWithExistingId_Throws_DuplicateIdException()
        {
            // Arrange
            Subject patient = new(SubjectId.New(), "John Doe");

            BloodPressureId measureId = BloodPressureId.New();
            patient.AddBloodPressure(measureId, 18.April(2012).AsUtc().ToInstant(), systolic: 120, diastolic: 50);

            // Act
            Action addMeasureWithDuplicateId = () => patient.AddBloodPressure(measureId, 18.April(2013).AsUtc().ToInstant(), systolic: 130, diastolic: 90);

            // Assert
            addMeasureWithDuplicateId.Should()
                .ThrowExactly<DuplicateIdException>("a measure with the same id already exists");
        }

        [Fact]
        public void RemoveExistingBloodPressure_Should_Remove_TheMeasure()
        {
            // Arrange
            Subject patient = new(SubjectId.New(), "John Doe");
            BloodPressureId measureId = BloodPressureId.New();

            patient.AddBloodPressure(measureId, 10.April(2014).AsUtc().ToInstant(), systolic: 120, diastolic: 80);

            // Act
            patient.DeleteBloodPressure(measureId);

            // Assert
            patient.BloodPressures.Should()
                .BeEmpty("The corresponding measure should have been removed");
        }
    }
}

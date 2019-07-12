using System;
using Xunit;
using FluentAssertions;
using FluentAssertions.Extensions;
using System.Collections.Generic;
using Xunit.Categories;
using Measures.Objects.Exceptions;

namespace Measures.Objects.Tests
{
    [UnitTest]
    public class PatientTests
    {
        [Fact]
        public void Ctor_Should_Throws_ArgumentOutRangeException_When_UUID_IsEmpty()
        {
            // Act
            Action ctor = () => new Patient(Guid.Empty);

            // Assert
            ctor.Should()
                .ThrowExactly<ArgumentOutOfRangeException>($"{nameof(Patient)}.{nameof(Patient.Id)} must not be empty");
        }

        [Fact]
        public void Ctor_Should_Create_Valid_Instance()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            // Act
            Patient expected = new Patient(id);

            // Assert
            expected.Id.Should()
                .Be(id);
            expected.Name.Should()
                .Be($"patient-{id}", $"{nameof(Patient)}.{nameof(Patient.Name)} must be computed by default");

            expected.Measures.Should()
                .BeEmpty();
        }

        [Theory]
        [InlineData("J'onzz J'onzz", "J'onzz J'onzz", "The new name is set as is because there no leading or trailing spaces")]
        [InlineData("clark kent", "Clark Kent", "The new name is set using proper case.")]
        [InlineData("", "", "Changing name to string.empty is allowed")]
        [InlineData("  ", "", "Changing name to string.empty is allowed")]
        [InlineData("clark kent  ", "Clark Kent", "ChangeName remove leading to string.empty is allowed")]
        public void ChangeName_Set_Name_Property(string newName, string expected, string reason)
        {
            // Arrange
            Patient patient = new Patient(Guid.NewGuid());

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
            Patient patient = new Patient(Guid.NewGuid());
            patient.ChangeNameTo("clark kent");

            // Act
            Action changingName = () => patient.ChangeNameTo(null);

            // Assert
            patient.Name.Should()
                .Be("Clark Kent");
            changingName.Should()
                .ThrowExactly<ArgumentNullException>($"{nameof(Patient)}.{nameof(Patient.Name)} cannot be set to null.");
        }

        public static IEnumerable<object[]> AddBloodPressureCases
        {
            get
            {
                yield return new object[] { Guid.NewGuid(), 10.April(2012), 120, 80 };
            }
        }

        [Theory]
        [MemberData(nameof(AddBloodPressureCases))]
        public void AddingBloodPressure_Should_AddMeasure(Guid measureId, DateTimeOffset dateOfMeasure, float systolic, float diastolic)
        {
            // Arrange
            Patient patient = new Patient(Guid.NewGuid());

            // Act
            patient.AddBloodPressure(measureId, dateOfMeasure , systolic, diastolic);

            // Assert
            BloodPressure measure = patient.Measures.Should()
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
            Patient patient = new Patient(Guid.NewGuid());

            // Act
            Action addWithNoId = () => patient.AddBloodPressure(Guid.Empty, 22.April(2014), systolic: 150, diastolic: 80);

            // Assert
            addWithNoId.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("id of the measure must be provided and it cannot be empty");
        }

        [Fact]
        public void AddingBloodPressureWithExistingId_Throws_DuplicateIdException()
        {
            // Arrange
            Patient patient = new Patient(Guid.NewGuid());

            Guid measureId = Guid.NewGuid();
            patient.AddBloodPressure(measureId, 18.April(2012), systolic: 120, diastolic: 50);

            // Act
            Action addMeasureWithDuplicateId = () => patient.AddBloodPressure(measureId, 18.April(2013), systolic: 130 , diastolic: 90);

            // Assert
            addMeasureWithDuplicateId.Should()
                .ThrowExactly<DuplicateIdException>("a measure with the same id already exists");
        }

        [Fact]
        public void RemoveExistingBloodPressure_Should_Remove_TheMeasure()
        {
            // Arrange
            Patient patient = new Patient(Guid.NewGuid());
            Guid measureId = Guid.NewGuid();

            patient.AddBloodPressure(measureId, 10.April(2014), systolic : 120, diastolic : 80);

            // Act
            patient.DeleteMeasure(measureId);

            // Assert
            patient.Measures.Should()
                .BeEmpty("The corresponding measure should have been removed");
        }
    }
}

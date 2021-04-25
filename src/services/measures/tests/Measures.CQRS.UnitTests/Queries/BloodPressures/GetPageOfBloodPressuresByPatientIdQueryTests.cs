using Bogus;

using FluentAssertions;

using Measures.CQRS.Queries.BloodPressures;
using Measures.Ids;

using MedEasy.RestObjects;

using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Measures.CQRS.UnitTests.Queries.BloodPressures
{
    [Feature("Measures")]
    [UnitTest]
    public class GetPageOfBloodPressuresByPatientIdQueryTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public GetPageOfBloodPressuresByPatientIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> CtorWithIndividualParameterCases
        {
            get
            {
                yield return new object[]
                {
                    PatientId.Empty, null,
                    $"PatientId.Empty is not valid for patientId and null is not valid for pagination arguments"
                };

                yield return new object[]
                {
                    PatientId.New(), null,
                    $"null not a valid argument for patientId"
                };

                yield return new object[]
                {
                    PatientId.Empty, new PaginationConfiguration(),
                    $"PatientId.Empty is not valid for patientId"
                };
            }
        }

        [Theory]
        [MemberData(nameof(CtorWithIndividualParameterCases))]
        public void GivenDefaultParameters_Ctor_ThrowsArgumentOutOfRangeException(PatientId patientId, PaginationConfiguration pagination, string reason)
        {
            // Act
            Action action = () => new GetPageOfBloodPressureInfoByPatientIdQuery(patientId, pagination);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>(reason).Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> CtorWithTupleCases
        {
            get
            {
                Faker faker = new();
                yield return new object[]
                {
                    null,
                    new PaginationConfiguration { Page = faker.Random.Int(min:1), PageSize = faker.Random.Int(min:1) },
                    $"{default((PatientId, PaginationConfiguration))} is not a valid argument"
                };

                yield return new object[]
                {
                    PatientId.Empty,
                    null,
                    $"{nameof(PatientId)} cannot be empty"
                };
            }
        }

        [Theory]
        [MemberData(nameof(CtorWithTupleCases))]
        public void GivenDefaultParameters_CtorWithTuple_ThrowsArgumentOutOfRangeException(PatientId patientId, PaginationConfiguration pagination, string reason)
        {
            
            // Act
            Action action = () => new GetPageOfBloodPressureInfoByPatientIdQuery(patientId, pagination);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>(reason).Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GivenSameData_TwoInstances_Have_DifferentIds()
        {
            // Arrange
            PatientId patientId = PatientId.New();
            PaginationConfiguration pagination = new();

            // Act
            GetPageOfBloodPressureInfoByPatientIdQuery first = new(patientId, pagination);
            GetPageOfBloodPressureInfoByPatientIdQuery second = new(patientId, pagination);

            first.Id.Should()
                .NotBeEmpty();
            second.Id.Should()
                .NotBeEmpty();

            first.Should()
                .NotBeSameAs(second);
        }
    }
}

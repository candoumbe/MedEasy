using Bogus;

using FluentAssertions;

using FsCheck;
using FsCheck.Xunit;

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

        public static IEnumerable<object[]> CtorWithTupleCases
        {
            get
            {
                Faker faker = new();
                yield return new object[]
                {
                    null,
                    new PaginationConfiguration { Page = faker.Random.Int(min:1), PageSize = faker.Random.Int(min:1) },
                    $"null is not a valid value for {nameof(PatientId)}"
                };

                yield return new object[]
                {
                    PatientId.Empty,
                    new PaginationConfiguration { Page = faker.Random.Int(min:1), PageSize = faker.Random.Int(min:1) },
                    $"{nameof(PatientId)}.{nameof(PatientId.Empty)} is not a valid value for {nameof(PatientId)}"
                };

                yield return new object[]
                {
                    PatientId.New(),
                    null,
                    $"null is not a valid value for {nameof(PaginationConfiguration)}"
                };
            }
        }

        [Property]
        public void Given_PatientId_is_null_constructor_should_throw_ArgumentNullException(PositiveInt page, PositiveInt pageSize)
        {
            // Arrange
            PaginationConfiguration pagination = new PaginationConfiguration { Page = page.Item, PageSize = pageSize.Item };

            // Act
            Action action = () => new GetPageOfBloodPressureInfoByPatientIdQuery(null, pagination);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Property]
        public void Given_PatientId_is_empty_constructor_should_throw_ArgumentOutOfRangeException(PositiveInt page, PositiveInt pageSize)
        {
            // Arrange
            PaginationConfiguration pagination = new PaginationConfiguration { Page = page.Item, PageSize = pageSize.Item };

            // Act
            Action action = () => new GetPageOfBloodPressureInfoByPatientIdQuery(PatientId.Empty, pagination);

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Given_pagination_is_null_constructor_should_throw_ArgumentNullException()
        {
            // Act
            Action action = () => new GetPageOfBloodPressureInfoByPatientIdQuery(PatientId.New(), null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Property]
        public Property Given_samedata_two_instances_have_different_Id(Guid patientGuidId, PositiveInt page, PositiveInt pageSize)
        {
            // Arrange
            PatientId id = new (patientGuidId);
            PaginationConfiguration pagination = new PaginationConfiguration { Page = page.Item, PageSize = pageSize.Item };

            // Act
            GetPageOfBloodPressureInfoByPatientIdQuery first = new(id, pagination);
            GetPageOfBloodPressureInfoByPatientIdQuery second = new(id, pagination);

            return (first.Id != Guid.Empty
                    && second.Id != Guid.Empty
                    && first.Id != second.Id).ToProperty();
        }
    }
}

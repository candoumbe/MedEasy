namespace Measures.CQRS.UnitTests.Queries.BloodPressures
{
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
                    $"null is not a valid value for {nameof(SubjectId)}"
                };

                yield return new object[]
                {
                    SubjectId.Empty,
                    new PaginationConfiguration { Page = faker.Random.Int(min:1), PageSize = faker.Random.Int(min:1) },
                    $"{nameof(SubjectId)}.{nameof(SubjectId.Empty)} is not a valid value for {nameof(SubjectId)}"
                };

                yield return new object[]
                {
                    SubjectId.New(),
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
            Action action = () => new GetPageOfBloodPressureInfoBySubjectIdQuery(null, pagination);

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
            Action action = () => new GetPageOfBloodPressureInfoBySubjectIdQuery(SubjectId.Empty, pagination);

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
            Action action = () => new GetPageOfBloodPressureInfoBySubjectIdQuery(SubjectId.New(), null);

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
            SubjectId id = new (patientGuidId);
            PaginationConfiguration pagination = new PaginationConfiguration { Page = page.Item, PageSize = pageSize.Item };

            // Act
            GetPageOfBloodPressureInfoBySubjectIdQuery first = new(id, pagination);
            GetPageOfBloodPressureInfoBySubjectIdQuery second = new(id, pagination);

            return (first.Id != Guid.Empty
                    && second.Id != Guid.Empty
                    && first.Id != second.Id).ToProperty();
        }
    }
}

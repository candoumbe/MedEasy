namespace MedEasy.CQRS.Core.UnitTests.Queries
{
    using FluentAssertions;

    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DTO.Search;

    using System;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class SearchQueryTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;

        public class SuperHero
        {
            public string Nickname { get; set; }

            public string Powers { get; set; }

            public int YearOfBirth { get; set; }
        }

        public SearchQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
        }

        [Fact]
        public void Ctor_Create_Valid_Instance()
        {
            // Arrange
            SearchQueryInfo<SuperHero> info = new();

            // Act
            SearchQuery<SuperHero> instance = new(info);

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
            instance.Data.Should()
                .BeSameAs(info);
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Parameter_Is_Null()
        {
            // Act
            Action action = () => new SearchQuery<SuperHero>(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }
    }
}

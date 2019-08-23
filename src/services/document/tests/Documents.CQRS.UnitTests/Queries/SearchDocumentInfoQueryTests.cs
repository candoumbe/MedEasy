using Documents.CQRS.Queries;
using Documents.DTO.v1;
using FluentAssertions;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using System;
using Xunit;
using Xunit.Categories;

namespace Documents.CQRS.UnitTests.Queries
{
    [UnitTest]
    [Feature(nameof(Documents))]
    public class SearchDocumentInfoQueryTests
    {
        [Fact]
        public void IsAQuery() => typeof(SearchDocumentInfoQuery).Should()
            .BeAssignableTo<IQuery<Guid, SearchDocumentInfo, Page<DocumentInfo>>>().And
            .NotBeAbstract();

        [Fact]
        public void GivenNullParameter_Ctor_ThrowsArgumentNullException()
        {
            // Act
            Action action = () => new SearchDocumentInfoQuery(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().And
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}

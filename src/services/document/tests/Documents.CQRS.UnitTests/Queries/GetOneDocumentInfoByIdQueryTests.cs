namespace Documents.CQRS.UnitTests.Queries
{
    using Documents.CQRS.Queries;
    using Documents.DTO.v1;
    using Documents.Ids;

    using FluentAssertions;

    using MedEasy.CQRS.Core.Queries;

    using Optional;

    using System;

    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Documents")]
    public class GetOneDocumentInfoByIdQueryTests
    {
        public GetOneDocumentInfoByIdQueryTests()
        {
        }

        [Fact]
        public void IsQuery() => typeof(GetOneDocumentInfoByIdQuery).Should()
            .BeAssignableTo<IQuery<Guid, DocumentId, Option<DocumentInfo>>>();

        [Fact]
        public void Has_A_Uniuque_Identifier()
        {
            // Arrange
            DocumentId documentId = DocumentId.New();

            // Act
            GetOneDocumentInfoByIdQuery instance = new(documentId);

            // Assert
            instance.Id.Should()
                .NotBeEmpty().And
                .NotBe(documentId.Value);


        }
    }
}

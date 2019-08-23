using Documents.CQRS.Queries;
using Documents.DTO.v1;
using FluentAssertions;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;
using Xunit;

namespace Documents.CQRS.UnitTests.Queries
{
    public class GetOneDocumentFileInfoByIdQueryTests
    {
        public GetOneDocumentFileInfoByIdQueryTests()
        {

        }

        [Fact]
        public void IsQuery() => typeof(GetOneDocumentFileInfoByIdQuery).Should()
            .BeAssignableTo<IQuery<Guid, Guid, Option<(DocumentInfo, byte[])>>>();

        [Fact]
        public void Has_A_Uniuque_Identifier()
        {
            // Arrange
            Guid documentId = Guid.NewGuid();

            // Act
            GetOneDocumentFileInfoByIdQuery instance = new GetOneDocumentFileInfoByIdQuery(documentId);

            // Assert
            instance.Id.Should()
                .NotBeEmpty().And
                .NotBe(documentId);


        }
    }
}

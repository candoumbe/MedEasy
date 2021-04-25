using Documents.CQRS.Queries;
using Documents.DTO.v1;
using Documents.Ids;

using FluentAssertions;

using MedEasy.CQRS.Core.Queries;

using System;
using System.Collections.Generic;

using Xunit;

namespace Documents.CQRS.UnitTests.Queries
{
    public class GetOneDocumentFileInfoByIdQueryTests
    {

        [Fact]
        public void IsQuery() => typeof(GetOneDocumentFileInfoByIdQuery).Should()
            .BeAssignableTo<IQuery<Guid, DocumentId, IAsyncEnumerable<DocumentPartInfo>>>();

        [Fact]
        public void Has_A_Unique_Identifier()
        {
            // Arrange
            DocumentId documentId = DocumentId.New();

            // Act
            GetOneDocumentFileInfoByIdQuery instance = new(documentId);

            // Assert
            instance.Id.Should()
                .NotBe(Guid.Empty).And
                .NotBe(documentId.Value);
        }
    }
}

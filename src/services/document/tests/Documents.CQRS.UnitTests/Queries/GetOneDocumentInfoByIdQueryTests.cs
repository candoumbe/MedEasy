﻿using Documents.CQRS.Queries;
using Documents.DTO.v1;
using FluentAssertions;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;
using Xunit;

namespace Documents.CQRS.UnitTests.Queries
{
    public class GetOneDocumentInfoByIdQueryTests
    {
        public GetOneDocumentInfoByIdQueryTests()
        {

        }

        [Fact]
        public void IsQuery() => typeof(GetOneDocumentInfoByIdQuery).Should()
            .BeAssignableTo<IQuery<Guid, Guid, Option<DocumentInfo>>>();

        [Fact]
        public void Has_A_Uniuque_Identifier()
        {
            // Arrange
            Guid documentId = Guid.NewGuid();

            // Act
            GetOneDocumentInfoByIdQuery instance = new GetOneDocumentInfoByIdQuery(documentId);

            // Assert
            instance.Id.Should()
                .NotBeEmpty().And
                .NotBe(documentId);


        }
    }
}

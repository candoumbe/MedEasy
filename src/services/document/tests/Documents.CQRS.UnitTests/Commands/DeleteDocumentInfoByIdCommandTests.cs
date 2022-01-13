namespace Documents.CQRS.UnitTests.Commands
{
    using Documents.CQRS.Commands;
    using Documents.Ids;

    using FluentAssertions;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using System;

    using Xunit;
    using Xunit.Categories;

    [Feature("Documents")]
    [UnitTest]
    public class DeletedocumentByIdCommandTests
    {
        [Fact]
        public void IsCommand() => typeof(DeleteDocumentInfoByIdCommand).Should()
           .BeAssignableTo<ICommand<Guid, DocumentId, DeleteCommandResult>>();


        [Fact]
        public void GivenEmptyGuid_Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new DeleteDocumentInfoByIdCommand(DocumentId.Empty);

            // Assert
            action.Should()
                .Throw<ArgumentException>();
        }
    }
}

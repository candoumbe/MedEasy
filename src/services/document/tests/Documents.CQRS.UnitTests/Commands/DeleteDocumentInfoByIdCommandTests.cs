using Documents.CQRS.Commands;
using FluentAssertions;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;
using Xunit;
using Xunit.Categories;

namespace Documents.CQRS.UnitTests.Commands
{
    [Feature("Agenda")]
    [UnitTest]
    public class DeleteAppointmentInfoByIdCommandTests
    {

        [Fact]
        public void IsCommand() => typeof(DeleteDocumentInfoByIdCommand).Should()
           .BeAssignableTo<ICommand<Guid, Guid, DeleteCommandResult>>();


        [Fact]
        public void GivenEmptyGuid_Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new DeleteDocumentInfoByIdCommand(Guid.Empty);

            // Assert
            action.Should()
                .Throw<ArgumentException>();
        }
    }
}

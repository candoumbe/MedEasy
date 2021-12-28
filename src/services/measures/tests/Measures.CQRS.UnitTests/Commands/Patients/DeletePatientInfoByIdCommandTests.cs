namespace Measures.CQRS.UnitTests.Commands.Patients
{
    using FluentAssertions;

    using Measures.CQRS.Commands.Patients;
    using Measures.Ids;

    using System;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Commands")]
    public class DeletePatientInfoByIdCommandTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public DeletePatientInfoByIdCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }


        [Fact]
        public void Ctor_Is_Valid()
        {
            DeletePatientInfoByIdCommand instance = new(SubjectId.New());

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new DeletePatientInfoByIdCommand(default);

            // Assert
            action.Should()
                .Throw<ArgumentException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}

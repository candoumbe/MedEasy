using FluentAssertions;
using Measures.CQRS.Commands.Patients;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Measures.CQRS.UnitTests.Commands.Patients
{
    [UnitTest]
    [Feature("Commands")]
    public class DeletePatientInfoByIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public DeletePatientInfoByIdCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
            _outputHelper = null;
        }

        [Fact]
        public void Ctor_Is_Valid()
        {
            DeletePatientInfoByIdCommand instance = new DeletePatientInfoByIdCommand(Guid.NewGuid());


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

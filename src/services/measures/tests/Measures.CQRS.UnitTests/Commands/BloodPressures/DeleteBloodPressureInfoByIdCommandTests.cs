using FluentAssertions;

using Measures.CQRS.Commands.BloodPressures;
using Measures.Ids;

using System;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Measures.CQRS.UnitTests.Commands.BloodPressures
{
    [UnitTest]
    public class DeleteBloodPressureInfoByIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public DeleteBloodPressureInfoByIdCommandTests(ITestOutputHelper outputHelper)
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
            DeleteBloodPressureInfoByIdCommand instance = new(BloodPressureId.New());

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new DeleteBloodPressureInfoByIdCommand(default);

            // Assert
            action.Should()
                .Throw<ArgumentException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}

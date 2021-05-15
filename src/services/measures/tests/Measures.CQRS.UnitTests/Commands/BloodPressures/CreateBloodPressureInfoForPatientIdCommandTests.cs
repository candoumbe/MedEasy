namespace Measures.CQRS.UnitTests.Commands.BloodPressures
{
    using FluentAssertions;

    using Measures.CQRS.Commands.BloodPressures;
    using Measures.DTO;

    using System;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class CreateBloodPressureInfoForPatientIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreateBloodPressureInfoForPatientIdCommandTests(ITestOutputHelper outputHelper)
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
            CreateBloodPressureInfoForPatientIdCommand instance = new(new CreateBloodPressureInfo()
            {
            });

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new CreateBloodPressureInfoForPatientIdCommand(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}
using FluentAssertions;
using Measures.CQRS.Commands.BloodPressures;
using Measures.DTO;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Measures.CQRS.UnitTests.Commands.BloodPressures
{
    [UnitTest]
    public class CreateBloodPressureInfoCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreateBloodPressureInfoCommandTests(ITestOutputHelper outputHelper)
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
            CreateBloodPressureInfoCommand instance = new CreateBloodPressureInfoCommand(new CreateBloodPressureInfo()
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
            Action action = () => new CreateBloodPressureInfoCommand(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();

        }
    }
}
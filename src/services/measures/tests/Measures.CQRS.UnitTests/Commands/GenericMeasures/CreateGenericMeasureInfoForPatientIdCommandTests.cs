using FluentAssertions;

using Measures.CQRS.Commands;
using Measures.CQRS.Commands.BloodPressures;
using Measures.DTO;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Measures.CQRS.UnitTests.Commands
{
    [UnitTest]
    public class CreateGenericMeasureInfoForPatientIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreateGenericMeasureInfoForPatientIdCommandTests(ITestOutputHelper outputHelper)
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
            CreateGenericMeasureInfoCommand instance = new CreateGenericMeasureInfoCommand(new CreateGenericMeasureInfo()
            {
            });

            // Assert
            instance.Id.Should()
                        .NotBeEmpty();
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException_when_data_is_null()
        {
            // Act
            Action action = () => new CreateGenericMeasureInfoCommand(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}
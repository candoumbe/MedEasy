using FluentAssertions;
using MedEasy.DTO;
using MedEasy.Objects;
using System;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Patient
{
    public class AddNewTemperatureMeasureCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public AddNewTemperatureMeasureCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }


        [Fact]
        public void ShouldThrowArgumentNullExceptionIfCommandToRunIsNull()
        {
            // Act 
            Action action = () => new AddNewPhysiologicalMeasureCommand<Temperature, TemperatureInfo>(null);

            // Assert

            action.ShouldThrow<ArgumentNullException>("the command's parameter cannot be null").Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace("it's usefull to debug quickly");
        }



        public void Dispose()
        {
            _outputHelper = null;
        }
    }
}

using FluentAssertions;
using MedEasy.DTO;
using MedEasy.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Patient
{
    public class AddNewBloodPressureMeasureCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public AddNewBloodPressureMeasureCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        
        public void Dispose()
        {
            _outputHelper = null;
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenPassingNullArgument()
        {
            // Act
            Action action = () => new AddNewPhysiologicalMeasureCommand<Temperature, TemperatureInfo>(null);


            // Assert 
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void CtorShouldInitiliazeDataCorrectly()
        {
            // Arrange
            
            CreatePhysiologicalMeasureInfo<Temperature> createCommandInfo = new CreatePhysiologicalMeasureInfo<Temperature>
            {
                Measure = new Temperature { Value = 25 },
                PatientId = Guid.NewGuid()
            };

            // Act
            AddNewPhysiologicalMeasureCommand<Temperature, TemperatureInfo> cmd = new AddNewPhysiologicalMeasureCommand<Temperature, TemperatureInfo>(createCommandInfo);

            // Assert
            cmd.Id.Should().NotBeEmpty();
            cmd.Data.ShouldBeEquivalentTo(createCommandInfo);
        }
    }
}

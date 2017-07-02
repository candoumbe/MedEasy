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



        [Fact]
        public void ShouldThrowArgumentNullExceptionIfCommandToRunIsNull()
        {
            // Act 
            Action action = () => new AddNewPhysiologicalMeasureCommand<BloodPressure, BloodPressureInfo>(null);

            // Assert

            action.ShouldThrow<ArgumentException>("the command's parameter cannot be null").Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace("a param name is required to ease the debugging process");
        }


        public void Dispose()
        {
            _outputHelper = null;
        }
    }
}

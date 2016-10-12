using FluentAssertions;
using System;
using Xunit;
using Xunit.Abstractions;
using MedEasy.Queries.Patient;
using MedEasy.DTO;

namespace MedEasy.Queries.Tests.Patient
{
    public class WantMostRecentPhysiologicalMeasuresQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public WantMostRecentPhysiologicalMeasuresQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
            _outputHelper = null;
        }


        [Fact]
        public void ConstructorWithNullArgumentShouldThrowsArgumentNullException()
        {
            // Act
            Action action = () => new WantMostRecentPhysiologicalMeasuresQuery<TemperatureInfo>(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>("parameter cannot be null").Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace("it's easier to debug");
        }
    }
}

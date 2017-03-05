using FluentAssertions;
using System;
using Xunit;
using Xunit.Abstractions;
using MedEasy.Queries.Patient;
using MedEasy.DTO;
using System.Linq.Expressions;

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


       [Fact]
       public void Ctor()
       {
            // Arrange
            GetMostRecentPhysiologicalMeasuresInfo input = new GetMostRecentPhysiologicalMeasuresInfo {
                Count = 10,
                PatientId = Guid.NewGuid()
            };


            _outputHelper.WriteLine($"Input : {input}");
            // Act
            WantMostRecentPhysiologicalMeasuresQuery<TemperatureInfo> instance = new WantMostRecentPhysiologicalMeasuresQuery<TemperatureInfo>(input);

            // Assert
            instance.Id.Should().NotBeEmpty("id will be used for logging");
            instance.Data.Should().NotBeNull();
            instance.Data.PatientId.Should().Be(input.PatientId);
            instance.Data.Count.Should().Be(input.Count);
        }


        }
}

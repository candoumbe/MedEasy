using FluentAssertions;
using MedEasy.Commands.Patient;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Patient
{
    public class DeleteOnePhysiologicalMeasureCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public DeleteOnePhysiologicalMeasureCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new DeleteOnePhysiologicalMeasureCommand(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void CtorShouldBuildUniqueInstanceOfCommand()
        {

            // Arrange
            Guid patientId = Guid.NewGuid();
            Guid measureId = Guid.NewGuid();
            DeletePhysiologicalMeasureInfo info = new DeletePhysiologicalMeasureInfo
            {
                Id = patientId,
                MeasureId = measureId
            };

            // Act
            DeleteOnePhysiologicalMeasureCommand first = new DeleteOnePhysiologicalMeasureCommand(info);
            DeleteOnePhysiologicalMeasureCommand second = new DeleteOnePhysiologicalMeasureCommand(info);

            // Assert
            first.Should().NotBeSameAs(second);
            first.Id.Should().NotBe(second.Id);
            first.Data.ShouldBeEquivalentTo(second.Data);
            first.Data.Should().NotBeSameAs(second.Data, $"two {nameof(DeleteOnePhysiologicalMeasureCommand)} instances built from shared data should not share state");
        }

        [Fact]
        public void CtorShouldBuildValidCommand()
        {

            // Arrange
            Guid patientId = Guid.NewGuid();
            Guid measureId = Guid.NewGuid();
            DeletePhysiologicalMeasureInfo info = new DeletePhysiologicalMeasureInfo
            {
                Id = patientId,
                MeasureId = measureId
            };

            // Act
            DeleteOnePhysiologicalMeasureCommand command = new DeleteOnePhysiologicalMeasureCommand(info);


            // Assert
            command.Id.Should().NotBeEmpty();
        }
    }
}

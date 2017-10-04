using FluentAssertions;
using MedEasy.Commands.Prescription;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Prescription
{
    public class CreatePrescriptionCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreatePrescriptionCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new CreatePrescriptionCommand(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void CtorShouldBuildUniqueInstanceOfCommand()
        {
            // Arrange
            Guid prescriptionId = Guid.NewGuid();
            CreatePrescriptionInfo info = new CreatePrescriptionInfo
            {
                Duration = 30,
                DeliveryDate = 7.July(2010),
                PrescriptorId = prescriptionId
                
            };

            // Act
            CreatePrescriptionCommand first = new CreatePrescriptionCommand(info);
            CreatePrescriptionCommand second = new CreatePrescriptionCommand(info);

            // Assert
            first.Should().NotBeSameAs(second);
            first.Id.Should().NotBe(second.Id);
            first.Data.ShouldBeEquivalentTo(second.Data);
            first.Data.Should().NotBeSameAs(second.Data, $"two {nameof(CreatePrescriptionCommand)} instances built from shared data should not share state.");
        }

        [Fact]
        public void CtorShouldBuildValidCommand()
        {

            // Arrange
            Guid prescriptionId = Guid.NewGuid();
            CreatePrescriptionInfo info = new CreatePrescriptionInfo
            {
                Duration = 30,
                DeliveryDate = 7.July(2010),
                PrescriptorId = prescriptionId

            };

            // Act
            CreatePrescriptionCommand command = new CreatePrescriptionCommand(info);


            // Assert
            command.Id.Should().NotBeEmpty();
            command.Data.ShouldBeEquivalentTo(info);
            command.Data.Should().NotBeSameAs(info, $"Risk of shared state. Use {nameof(ObjectExtensions.DeepClone)}<T>() to keep a copy of the command's data");
        }
    }
}

using MedEasy.CQRS.Core.Commands;
using Patients.Objects;
using System;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using MedEasy.DTO;
using Patients.DTO;

namespace MedEasy.CQRS.Core.Tests
{
    /// <summary>
    /// Unit tests for <see cref="PatchCommand"/> class
    /// </summary>
    public class PatchCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public PatchCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }
        public void Dispose() => _outputHelper = null;

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => new PatchCommand<Guid, Patient>(null);

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

            JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
            patchDocument.Replace(x => x.Lastname, "Banner");
            PatchInfo<Guid, PatientInfo> info = new PatchInfo<Guid, PatientInfo>
            {
                Id = patientId,
                PatchDocument = patchDocument
            };

            // Act
            PatchCommand<Guid, PatientInfo> first = new PatchCommand<Guid, PatientInfo>(info);
            PatchCommand<Guid, PatientInfo> second = new PatchCommand<Guid, PatientInfo>(info);

            // Assert
            first.Should().NotBeSameAs(second);
            first.Id.Should().NotBe(second.Id);
            first.Data.ShouldBeEquivalentTo(second.Data);
            first.Data.Should().NotBeSameAs(second.Data, $"two {nameof(PatchCommand<Guid, PatientInfo>)} instances built from shared data should not share state.");
        }
    }

}

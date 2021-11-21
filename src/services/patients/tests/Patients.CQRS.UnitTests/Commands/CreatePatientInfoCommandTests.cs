namespace Patients.CQRS.UnitTests.Commands
{
    using FluentAssertions;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using Optional;

    using Patients.CQRS.Commands;
    using Patients.DTO;

    using System;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Patients")]
    [Feature("Commands")]
    public class CreatePatientInfoCommandTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public CreatePatientInfoCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsCommand() => typeof(CreatePatientInfoCommand).Should()
            .Implement<ICommand<Guid, CreatePatientInfo, Option<PatientInfo, CreateCommandFailure>>>($"Commands must implement {nameof(ICommand<Guid, CreatePatientInfo, PatientInfo>)}").And
            .NotBeAbstract().And
            .NotHaveDefaultConstructor();

        [Fact]
        public void Ctor_throws_ArgumentNullException_when_parameter_is_null()
        {
            // Arrange
            Action action = () => new CreatePatientInfoCommand(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>()
                .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName), "Parameter name must be provided");
        }
    }
}

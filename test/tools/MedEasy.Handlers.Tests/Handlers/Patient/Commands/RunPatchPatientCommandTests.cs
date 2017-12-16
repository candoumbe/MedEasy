using FluentAssertions;
using MedEasy.DAL.Interfaces;
using MedEasy.RestObjects;
using MedEasy.Handlers.Patient.Commands;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using static FluentValidation.Severity;
using static Newtonsoft.Json.JsonConvert;
using MedEasy.Commands;
using Microsoft.AspNetCore.JsonPatch;
using MedEasy.Handlers.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;
using Optional;
using MedEasy.CQRS.Core;
using FluentValidation;
using FluentValidation.Results;
using System.Threading;

namespace MedEasy.Handlers.Tests.Handlers.Patient.Commands
{

    [Collection("Unit tests")]
    public class RunPatchPatientCommandTests : IDisposable
    {
        private RunPatchPatientCommand _commandRunner;
        private Mock<IValidator<IPatchCommand<Guid, Objects.Patient, PatchInfo<Guid, Objects.Patient>>>> _commandValidatorMock;
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<ILogger<RunPatchPatientCommand>> _loggerMock;


        public RunPatchPatientCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;


            DbContextOptionsBuilder<MedEasyContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MedEasyContext>()
                .UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _uowFactory = new EFUnitOfWorkFactory(dbContextOptionsBuilder.Options);

            _commandValidatorMock = new Mock<IValidator<IPatchCommand<Guid, Objects.Patient, PatchInfo<Guid, Objects.Patient>>>>(Strict);
            _loggerMock = new Mock<ILogger<RunPatchPatientCommand>>(Strict);

            _commandRunner = new RunPatchPatientCommand(_uowFactory);
        }

        public void Dispose()
        {
            _outputHelper = null;

            _uowFactory = null;
            _commandValidatorMock = null;

            _commandRunner = null;

            _loggerMock = null;
        }


        public static IEnumerable<object> CtorInvalidCases
        {
            get
            {
                yield return new object[] { null, null, null };
                yield return new object[] { null, null, new Mock<IValidator<IPatchCommand<Guid, Objects.Patient, PatchInfo<Guid, Objects.Patient>>>>().Object };
                yield return new object[] { new Mock<IUnitOfWorkFactory>().Object, null, null };
            }
        }

        [Fact]
        public void CtorShouldThrowArgumentNullException()
        {
            // Act
            Action action = () => new RunPatchPatientCommand(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>("parameter is null").Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }



        public static IEnumerable<object> PatchPatientCases
        {
            get
            {
                {
                    JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
                    patchDocument.Replace(x => x.MainDoctorId, 2);

                    Guid patientId = Guid.NewGuid();

                    yield return new object[]
                    {
                        new [] {
                            new Objects.Patient { Id = 1, MainDoctorId = null, UUID = patientId }
                        },
                        new [] {
                            new Objects.Doctor { Id = 3 },
                            new Objects.Doctor { Id = 2 }
                        },
                        new PatchInfo<Guid, Objects.Patient> {
                            Id = patientId,
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<Option<Nothing, CommandException>, bool>>)(x => x.HasValue))
                    };
                }
                {
                    JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
                    patchDocument.Replace(x => x.MainDoctorId, null);
                    Guid patientId = Guid.NewGuid();

                    yield return new object[]
                    {
                        new [] {
                            new Objects.Patient { Id = 1, MainDoctorId = 2, UUID = patientId }
                        },
                        new [] {
                            new Objects.Doctor { Id = 3 },
                            new Objects.Doctor { Id = 2 }
                        },
                        new PatchInfo<Guid, Objects.Patient> {
                            Id = patientId,
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<Option<Nothing, CommandException>, bool>>)(x => x.HasValue))
                    };
                }
            }
        }

        /// <summary>
        /// Tests for valid cases
        /// </summary>
        /// <param name="patients">Current patient store state</param>
        /// <param name="doctorsIdsBdd">Current doctor store state</param>
        /// <param name="commandInfo">Command to change the main doctor id of one patient</param>
        /// <param name="patientExpectation">Expectation on the output of the command</param>
        [Theory]
        [MemberData(nameof(PatchPatientCases))]
        public async Task ShouldPatchTheResource(IEnumerable<Objects.Patient> patients, IEnumerable<Objects.Doctor> doctors, PatchInfo<Guid, Objects.Patient> changes, Expression<Func<Option<Nothing, CommandException>, bool>> patientExpectation)
        {
            _outputHelper.WriteLine($"Current patient store state : {SerializeObject(patients)}");
            _outputHelper.WriteLine($"Current doctor store state : {SerializeObject(doctors)}");
            _outputHelper.WriteLine($"Patch info : {SerializeObject(changes)}");

            // Arrange

            using (IUnitOfWork uow = _uowFactory.New())
            {
                uow.Repository<Objects.Doctor>().Create(doctors);
                uow.Repository<Objects.Patient>().Create(patients);

                await uow.SaveChangesAsync();
            }

            _commandValidatorMock.Setup(mock => mock.ValidateAsync(It.IsAny<IPatchCommand<Guid, Objects.Patient>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
            IPatchCommand<Guid, Objects.Patient> command = new PatchCommand<Guid, Objects.Patient>(changes);
            Option<Nothing, CommandException> result = await _commandRunner.RunAsync(command);

            // Assert
            result.HasValue.Should().BeTrue();
            result.Should().Match(patientExpectation);

        }

        /// <summary>
        /// Tests that <see cref="RunPatchPatientCommand.RunAsync(IChangeMainDoctorCommand)"/> throws <see cref="CommandNotValidException{TCommandId}"/>
        /// when the validator returns <see cref="ErrorInfo"/>s with <see cref="ErrorInfo.Severity"/> equals to <see cref="Error"/>
        /// </summary>
        [Fact]
        public async Task ShouldThrowCommandNotValidExceptionWhenCommandValidationFails()
        {
            // Arrange
            _commandValidatorMock.Setup(mock => mock.ValidateAsync(It.IsAny<IPatchCommand<Guid, Objects.Patient>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(
                    new[] {
                        new ValidationFailure(nameof(Objects.Patient.Id), "ID of the resource cannot change")
                    }));


            // Act
            PatchInfo<Guid, Objects.Patient> patchInfo = new PatchInfo<Guid, Objects.Patient>
            {
                Id = Guid.NewGuid(),
                PatchDocument = new JsonPatchDocument<Objects.Patient>()
            };
        IPatchCommand<Guid, Objects.Patient> command = new PatchCommand<Guid, Objects.Patient>(patchInfo);
        Option<Nothing, CommandException> result = await _commandRunner.RunAsync(command);

        // Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(exception =>
            {
                exception.Should().BeOfType<CommandEntityNotFoundException>().Which
                    .Errors.Should()
                    .NotBeNull();
    });


        }


/// <summary>
/// Tests that <see cref="RunPatchPatientCommand.RunAsync(IChangeMainDoctorCommand)"/> throws <see cref="QueryNotFoundException"/>
/// </summary>
/// <remarks>
/// <see cref="QueryNotFoundException"/> should be thrown when there's no <see cref="Objects.Patient"/> with <c><see cref="Objects.Patient.Id"/> <see cref="ChangeMainDoctorIdInfo.PatientId"/> </c>.
/// </remarks>
[Fact]
public async Task ShouldThrowNotFoundExceptionIfPatientNotFound()
{
    // Arrange
    _commandValidatorMock.Setup(mock => mock.ValidateAsync(It.IsAny<IPatchCommand<Guid, Objects.Patient>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());


    // Act
    JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
    patchDocument.Replace(x => x.Firstname, "Bruce");

    PatchInfo<Guid, Objects.Patient> commandData = new PatchInfo<Guid, Objects.Patient>
    {
        Id = Guid.NewGuid(),
        PatchDocument = patchDocument
    };
    IPatchCommand<Guid, Objects.Patient> command = new PatchCommand<Guid, Objects.Patient>(commandData);
    Option<Nothing, CommandException> result = await _commandRunner.RunAsync(command);


    // Assert
    result.HasValue.Should().BeFalse();
    result.MatchNone(exception =>
    {
        exception.Should().BeOfType<CommandEntityNotFoundException>().Which
            .Message.Should()
                .NotBeNullOrWhiteSpace().And
                .ContainEquivalentOf($"<{command.Data.Id}> not found");

    });
}


/// <summary>
/// Tests that <see cref="RunPatchPatientCommand.RunAsync(IChangeMainDoctorCommand)"/> throws <see cref="QueryNotFoundException"/>
/// </summary>
/// <remarks>
/// <see cref="QueryNotFoundException"/> should be thrown when <see cref="ChangeMainDoctorIdInfo.NewDoctorId"/> is not <c>null</c> and there's no <see cref="Objects.Doctor"/> with <c><see cref="Objects.Doctor.Id"/> == <see cref="ChangeMainDoctorIdInfo.NewDoctorId"/> </c>.
/// </remarks>
[Fact]
public async Task ShouldThrowNotFoundExceptionIfDoctorNotFound()
{
    // Arrange
    _commandValidatorMock.Setup(mock => mock.ValidateAsync(It.IsAny<IPatchCommand<Guid, Objects.Patient>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());

    // Act
    JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
    patchDocument.Replace(x => x.MainDoctorId, 3);
    PatchInfo<Guid, Objects.Patient> data = new PatchInfo<Guid, Objects.Patient>
    {
        Id = Guid.NewGuid(),
        PatchDocument = patchDocument
    };
    IPatchCommand<Guid, Objects.Patient> command = new PatchCommand<Guid, Objects.Patient>(data);
    Option<Nothing, CommandException> result = await _commandRunner.RunAsync(command);


    // Assert
    result.HasValue.Should().BeFalse();
    result.MatchNone(exception =>
        exception.Should().BeOfType<CommandEntityNotFoundException>().Which
            .Message.Should().NotBeNullOrWhiteSpace());

}
    }
}

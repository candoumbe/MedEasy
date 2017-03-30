using FluentAssertions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.Validators;
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
using static Moq.Times;
using static MedEasy.Validators.ErrorLevel;
using static Newtonsoft.Json.JsonConvert;
using MedEasy.Commands;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using MedEasy.Handlers.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;

namespace MedEasy.Handlers.Tests.Handlers.Patient.Commands
{
    public class RunPatchPatientCommandTests : IDisposable
    {
        private RunPatchPatientCommand _commandRunner;
        private Mock<IValidate<IPatchCommand<Guid, Objects.Patient>>> _commandValidatorMock;
        private Mock<ILogger<RunPatchPatientCommand>> _loggerMock;
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        

        public RunPatchPatientCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;


            DbContextOptionsBuilder<MedEasyContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MedEasyContext>()
                .UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _uowFactory = new EFUnitOfWorkFactory(dbContextOptionsBuilder.Options);

            _loggerMock = new Mock<ILogger<RunPatchPatientCommand>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _commandValidatorMock = new Mock<IValidate<IPatchCommand<Guid, Objects.Patient>>>(Strict);
            
            _commandRunner = new RunPatchPatientCommand(_uowFactory, _loggerMock.Object, _commandValidatorMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;

            _loggerMock = null;
            _uowFactory = null;
            _commandValidatorMock = null;

            _commandRunner = null;
        }


        public static IEnumerable<object> CtorInvalidCases
        {
            get
            {
                yield return new object[] { null, new Mock<ILogger<RunPatchPatientCommand>>().Object, new Mock<IValidate<IPatchCommand<Guid, Objects.Patient>>>().Object };
                yield return new object[] { new Mock<IUnitOfWorkFactory>().Object, null, new Mock<IValidate<IPatchCommand<Guid, Objects.Patient>>>().Object};
                yield return new object[] { new Mock<IUnitOfWorkFactory>().Object, new Mock<ILogger<RunPatchPatientCommand>>().Object, null };
            }
        }

        [Theory]
        [MemberData(nameof(CtorInvalidCases))]
        public void CtorShouldThrowArgumentNullException(IUnitOfWorkFactory uowFactory, ILogger<RunPatchPatientCommand> logger, IValidate<IPatchCommand<Guid, Objects.Patient>> validator)
        {
            // Act
            Action action = () => new RunPatchPatientCommand(uowFactory, logger, validator);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
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
                        ((Expression<Func<Objects.Patient, bool>>)( x => x.UUID == patientId && x.MainDoctorId == 2 ))
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
                        ((Expression<Func<Objects.Patient, bool>>)( x => x.UUID == patientId && x.MainDoctorId == null ))
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
        public async Task ShouldPatchTheResource(IEnumerable<Objects.Patient> patients, IEnumerable<Objects.Doctor> doctors, IPatchInfo<Guid, Objects.Patient> changes, Expression<Func<Objects.Patient, bool>> patientExpectation)
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

            _commandValidatorMock.Setup(mock => mock.Validate(It.IsAny<IPatchCommand<Guid, Objects.Patient>>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>());

            // Act
            JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();            
            IPatchCommand<Guid, Objects.Patient> command = new PatchCommand<Guid, Objects.Patient>(changes);
            await _commandRunner.RunAsync(command);

            // Assert
            _commandValidatorMock.Verify(mock => mock.Validate(command), Once);
        }

        /// <summary>
        /// Tests that <see cref="RunPatchPatientCommand.RunAsync(IChangeMainDoctorCommand)"/> throws <see cref="CommandNotValidException{TCommandId}"/>
        /// when the validator returns <see cref="ErrorInfo"/>s with <see cref="ErrorInfo.Severity"/> equals to <see cref="Error"/>
        /// </summary>
        [Fact]
        public void ShouldThrowCommandNotValidExceptionWhenCommandValidationFails()
        {
            // Arrange
            _commandValidatorMock.Setup(mock => mock.Validate(It.IsAny<IPatchCommand<Guid, Objects.Patient>>()))
                .Returns(new[]
                {
                    Task.FromResult(new ErrorInfo(nameof(Objects.Patient.Id), "ID of the resource cannot change", Error))
                });


            // Act
            IPatchInfo<Guid, Objects.Patient> patchInfo = new PatchInfo<Guid, Objects.Patient>
            {
                Id = Guid.NewGuid(),
                PatchDocument = new JsonPatchDocument<Objects.Patient>()
            };
            IPatchCommand<Guid, Objects.Patient> command = new PatchCommand<Guid, Objects.Patient>(patchInfo);
            Func<Task> action = async () => await _commandRunner.RunAsync(command);

            // Assert
            CommandNotValidException<Guid> exception = action.ShouldThrow<CommandNotValidException<Guid>>().Which;

            exception.CommandId.Should().Be(command.Id);
            exception.Errors.Should()
                .NotBeNullOrEmpty().And
                .Contain(x => x.Key == nameof(Objects.Patient.Id));


        }


        /// <summary>
        /// Tests that <see cref="RunPatchPatientCommand.RunAsync(IChangeMainDoctorCommand)"/> throws <see cref="NotFoundException"/>
        /// </summary>
        /// <remarks>
        /// <see cref="NotFoundException"/> should be thrown when there's no <see cref="Objects.Patient"/> with <c><see cref="Objects.Patient.Id"/> <see cref="ChangeMainDoctorIdInfo.PatientId"/> </c>.
        /// </remarks>
        [Fact]
        public void ShouldThrowNotFoundExceptionIfPatientNotFound()
        {
            // Arrange
            _commandValidatorMock.Setup(mock => mock.Validate(It.IsAny<IPatchCommand<Guid, Objects.Patient>>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>());

           
            // Act
            JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
            patchDocument.Replace(x => x.Firstname, "Bruce");

            IPatchInfo<Guid, Objects.Patient> commandData = new PatchInfo<Guid, Objects.Patient>
            {
                Id = Guid.NewGuid(),
                PatchDocument = patchDocument
            };
            IPatchCommand<Guid, Objects.Patient> command = new PatchCommand<Guid, Objects.Patient>(commandData);
            Func<Task> action = async () => await _commandRunner.RunAsync(command);


            // Assert
            action.ShouldThrow<NotFoundException>().Which
                .Message.Should()
                    .NotBeNullOrWhiteSpace().And
                    .ContainEquivalentOf("patient");
        }


        /// <summary>
        /// Tests that <see cref="RunPatchPatientCommand.RunAsync(IChangeMainDoctorCommand)"/> throws <see cref="NotFoundException"/>
        /// </summary>
        /// <remarks>
        /// <see cref="NotFoundException"/> should be thrown when <see cref="ChangeMainDoctorIdInfo.NewDoctorId"/> is not <c>null</c> and there's no <see cref="Objects.Doctor"/> with <c><see cref="Objects.Doctor.Id"/> == <see cref="ChangeMainDoctorIdInfo.NewDoctorId"/> </c>.
        /// </remarks>
        [Fact]
        public void ShouldThrowNotFoundExceptionIfDoctorNotFound()
        {
            // Arrange
            _commandValidatorMock.Setup(mock => mock.Validate(It.IsAny<IPatchCommand<Guid, Objects.Patient>>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>());

            // Act
            JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
            patchDocument.Replace(x => x.MainDoctorId, 3);
            IPatchInfo<Guid, Objects.Patient> data = new PatchInfo<Guid, Objects.Patient>
            {
                Id = Guid.NewGuid(),
                PatchDocument = patchDocument
            };
            IPatchCommand<Guid, Objects.Patient> command = new PatchCommand<Guid, Objects.Patient>(data) ;
            Func<Task> action = async () => await _commandRunner.RunAsync(command);


            // Assert
            action.ShouldThrow<NotFoundException>().Which
                .Message.Should()
                    .BeEquivalentTo("Doctor <3> not found");
            
        }
    }
}

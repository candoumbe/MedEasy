using FluentAssertions;
using MedEasy.Commands;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.Validators.ErrorLevel;
using static MedEasy.DTO.ChangeInfoType;

namespace MedEasy.Validators.Tests
{
    public class ValidatePatchPatientCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private ValidatePatchPatientCommand _validator;

        public ValidatePatchPatientCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _validator = new ValidatePatchPatientCommand();
        }

        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
        }


        public static IEnumerable<object> PatchCommandCases
        {
            get
            {
                yield return new object[]
                {
                    new PatchInfo<int>(),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x => 
                        x.Count() == 2 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<int>.Id)) && 
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<int>.Changes)))
                    ),
                    "Id of resource to patch not set and no change to make"
                };

                yield return new object[]
                {
                    new PatchInfo<int>
                    {
                        Id = 1,
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<int>.Changes)))
                    ),
                    "no change to make"
                };


                yield return new object[]
                {
                    new PatchInfo<int>
                    {
                        Id = 1,
                        Changes = new []
                        {
                            new ChangeInfo { Op = Update, Path = $"/{nameof(PatientInfo.MainDoctorId)}", Value = 1 },
                            new ChangeInfo { Op = Update, Path = $"/{nameof(PatientInfo.MainDoctorId)}", Value = 2 }
                        }
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatientInfo.MainDoctorId)))
                    ),
                    "Two changes set for the same operation but with different values is not a valid command"
                };


                yield return new object[]
                {
                    new PatchInfo<int>
                    {
                        Id = 1,
                        Changes = new []
                        {
                            new ChangeInfo { Op = Add, Path = $"/{nameof(PatientInfo.MainDoctorId)}", Value = 1 },
                            new ChangeInfo { Op = Update, Path = $"/{nameof(PatientInfo.MainDoctorId)}", Value = 2 }
                        }
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatientInfo.MainDoctorId)))
                    ),
                    "multiple changes for the same path"
                };

                yield return new object[]
                {
                    new PatchInfo<int>
                    {
                        Id = 1,
                        Changes = new []
                        {
                            new ChangeInfo { Op = Update, Path = $"//{nameof(PatientInfo.MainDoctorId)}", Value = 1 }
                        }
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(ChangeInfo.Path)))
                    ),
                    $"multiple '/' at the beginning of a {nameof(ChangeInfo.Path)} is not valid"
                };

                yield return new object[]
                {
                    new PatchInfo<int>
                    {
                        Id = 1,
                        Changes = new []
                        {
                            new ChangeInfo { Op = Update, Path = $"", Value = 1 }
                        }
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(ChangeInfo.Path)))
                    ),
                    $"Empty {nameof(ChangeInfo.Path)} is not valid"
                };

                yield return new object[]
                {
                    new PatchInfo<int>
                    {
                        Id = 1,
                        Changes = new []
                        {
                            new ChangeInfo { Op = Update, Path = "/", Value = 1 }
                        }
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(ChangeInfo.Path)))
                    ),
                    $"Empty {nameof(ChangeInfo.Path)} is not valid"
                };

                yield return new object[]
                {
                    new PatchInfo<int>
                    {
                        Id = 1,
                        Changes = new []
                        {
                            new ChangeInfo { Op = Update, Path = "/   ", Value = 1 }
                        }
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(ChangeInfo.Path)))
                    ),
                    $"Whitespace only '{nameof(ChangeInfo.Path)}' is not valid"
                };

                yield return new object[]
                {
                    new PatchInfo<int>
                    {
                        Id = 1,
                        Changes = new []
                        {
                            new ChangeInfo { Op = Update, Path = "/Toto", Value = 1 }
                        }
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Key == nameof(ChangeInfo.Path) && error.Description == "Unknown property 'Toto'" && error.Severity == Error))
                    ),
                    $"Whitespace only '{nameof(ChangeInfo.Path)}' is not a property of {nameof(PatientInfo)}"
                };


            }
        }

        [Theory]
        [MemberData(nameof(PatchCommandCases))]
        public async Task Validate(IPatchInfo<int> data, Expression<Func<IEnumerable<ErrorInfo>, bool>> errorsExpectation, string reason)
        {
            // Arrange
            IPatchCommand<int> command = new PatchCommand<int>(data);
            _outputHelper.WriteLine($"Command to validate : {command} ");

            // Act
            IEnumerable<Task<ErrorInfo>> errorsTasks = _validator.Validate(command);

            // Assert
            errorsTasks.Should().NotBeNull();
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(errorsTasks);
            errors.Should().Match(errorsExpectation, reason);

        }
    }
}

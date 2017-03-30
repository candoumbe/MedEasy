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
using Microsoft.AspNetCore.JsonPatch;

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
                    new PatchInfo<Guid, Objects.Patient>(),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 2 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<Guid, Objects.Patient>.Id)) &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<Guid, Objects.Patient>.PatchDocument)))
                    ),
                    "Id of resource to patch not set and no change to make"
                };

                yield return new object[]
                {
                    new PatchInfo<Guid, Objects.Patient>
                    {
                        Id = Guid.NewGuid(),
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<Guid, Objects.Patient>.PatchDocument)))
                    ),
                    "no change to make"
                };


                {
                    JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
                    patchDocument.Replace(x => x.Id, 1);

                    yield return new object[]
                    {
                        new PatchInfo<Guid, Objects.Patient>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                            x.Count() == 1 &&
                            x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<Guid, Objects.Patient>.PatchDocument)))
                        ),
                        "Cannot change the ID of the resource"
                    };
                }

                {
                    JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
                    patchDocument.Replace(x => x.UUID, Guid.NewGuid());

                    yield return new object[]
                    {
                        new PatchInfo<Guid, Objects.Patient>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                            x.Count() == 1 &&
                            x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<Guid, Objects.Patient>.PatchDocument)))
                        ),
                        "Cannot change the UUID of the resource"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(PatchCommandCases))]
        public async Task Validate(IPatchInfo<Guid, Objects.Patient> data, Expression<Func<IEnumerable<ErrorInfo>, bool>> errorsExpectation, string reason)
        {
            // Arrange
            IPatchCommand<Guid, Objects.Patient> command = new PatchCommand<Guid, Objects.Patient>(data);
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

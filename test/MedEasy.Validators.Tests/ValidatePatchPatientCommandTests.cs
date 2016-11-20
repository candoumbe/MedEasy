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
                    new PatchInfo<int, Objects.Patient>(),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 2 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<int, Objects.Patient>.Id)) &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<int, Objects.Patient>.PatchDocument)))
                    ),
                    "Id of resource to patch not set and no change to make"
                };

                yield return new object[]
                {
                    new PatchInfo<int, Objects.Patient>
                    {
                        Id = 1,
                    },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                        x.Count() == 1 &&
                        x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<int, Objects.Patient>.PatchDocument)))
                    ),
                    "no change to make"
                };


                {
                    JsonPatchDocument<Objects.Patient> patchDocument = new JsonPatchDocument<Objects.Patient>();
                    patchDocument.Replace(x => x.Id, 1);

                    yield return new object[]
                    {
                        new PatchInfo<int, Objects.Patient>
                        {
                            Id = 1,
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)(x =>
                            x.Count() == 1 &&
                            x.Once(error => error.Severity == Error && error.Key == nameof(PatchInfo<int, Objects.Patient>.PatchDocument)))
                        ),
                        "Cannot update the ID of the resource"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(PatchCommandCases))]
        public async Task Validate(IPatchInfo<int, Objects.Patient> data, Expression<Func<IEnumerable<ErrorInfo>, bool>> errorsExpectation, string reason)
        {
            // Arrange
            IPatchCommand<int, Objects.Patient> command = new PatchCommand<int, Objects.Patient>(data);
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

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MedEasy.DTO;
using MedEasy.Validators.Patch;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FluentValidation.Severity;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Validators.Tests
{
    /// <summary>
    /// Unit tests collection for <see cref="PatchPatientInfoValidator"/> class.
    /// </summary>
    public class PatchPatientInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        private IValidator<PatchInfo<Guid, PatientInfo>> Validator { get; set; }

        public PatchPatientInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            Validator = new PatchPatientInfoValidator();
        }

        public void Dispose()
        {
            _outputHelper = null;
            Validator = null;
        }


        public static IEnumerable<object[]> InvalidPatchInfoCases
        {
            get
            {
                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo>(),
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 2
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.Id) && x.Severity == Error)
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.Id)} and {nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} not set."
                };

                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo> { Id = Guid.Empty },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 2
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.Id) && x.Severity == Error)
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.Id)} == Guid.Empty and {nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} not set."
                };

                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo> { PatchDocument = null },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 2
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.Id) && x.Severity == Error)
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.Id)} and {nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} set to null."
                };

                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo> {Id = Guid.Empty, PatchDocument = null },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 2
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.Id) && x.Severity == Error)
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.Id)} set to Guid.Empty and {nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} set to null."
                };

                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo> {Id = Guid.NewGuid(), PatchDocument = new JsonPatchDocument<PatientInfo>() },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(x => x.ErrorMessage == "Operations must have at least one item." && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} has zero operation."
                };

                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.Id, Guid.Empty);

                    yield return new object[]
                    {
                        new PatchInfo<Guid, PatientInfo>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                        )),
                        $"{nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} contains an operation which replace {nameof(PatientInfo.Id)} with Guid.Empty."
                    };
                }
            }
        }

        /// <summary>
        /// Tests that a patch info which <c><see cref="PatchInfo{TResourceId, TResource}.Id"/> == <see cref="Guid.Empty"/></c>
        /// is not valid.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(InvalidPatchInfoCases))]
        public async Task Should_Fails(PatchInfo<Guid, PatientInfo> info, Expression<Func<ValidationResult, bool>> expectation, string because)
        {
            _outputHelper.WriteLine($"{nameof(info)} : {SerializeObject(info)}");

            // Act
            ValidationResult vr = await Validator.ValidateAsync(info);

            // Assert
            vr.Should().Match(expectation, because);
        }

        [Fact]
        public async Task Should_Fails_When_Id_Is_Unknown_Resource()
        {

            // Arrange
            PatchInfo<Guid, PatientInfo> info = new PatchInfo<Guid, PatientInfo>
            {
                Id = Guid.NewGuid(),
                PatchDocument = new JsonPatchDocument<PatientInfo>()
            };
        }
    }
}

using FluentAssertions;
using FluentValidation.Results;
using MedEasy.DTO;
using MedEasy.Validators.Patch;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using static Newtonsoft.Json.JsonConvert;
using static FluentValidation.Severity;

namespace MedEasy.Validators.Tests.Patch
{
    public class JsonPatchDocumentValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private JsonPatchDocumentValidator<PatientInfo> _validator;

        public JsonPatchDocumentValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _validator = new JsonPatchDocumentValidator<PatientInfo>();
        }


        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
        }


        public static IEnumerable<object[]> InvalidCases
        {
            get
            {
                yield return new object[]
                {
                    new JsonPatchDocument<PatientInfo>(),
                    ((Expression<Func<ValidationResult, bool>>)(
                        vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<PatientInfo>.Operations) && x.Severity == Error)
                    )),
                    "Patch document has no operations"
                };

                {
                    JsonPatchDocument<PatientInfo> patch = new JsonPatchDocument<PatientInfo>();
                    patch.Replace(x => x.Lastname, string.Empty);
                    patch.Replace(x => x.Lastname, string.Empty);
                    yield return new object[]
                    {
                        patch,
                        ((Expression<Func<ValidationResult, bool>>)(
                            vr => !vr.IsValid
                                && vr.Errors.Count == 1
                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<PatientInfo>.Operations) 
                                    && x.Severity == Error
                                    && x.ErrorMessage == $"Multiple operations on the same path."
                                )
                        )),
                        "Patch document has multiple operations for the same path"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(InvalidCases))]
        public void ShouldFails(JsonPatchDocument<PatientInfo> patchDocument, Expression<Func<ValidationResult, bool>> expectation, string because)
        {
            _outputHelper.WriteLine($"{nameof(patchDocument)} : {SerializeObject(patchDocument)}");

            // Act
            ValidationResult vr = _validator.Validate(patchDocument);

            // Assert
            vr.Should().Match(expectation, because);

        }
    }
}

using FluentAssertions;
using FluentValidation.Results;
using MedEasy.RestObjects;
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

        public class SuperHero
        {
            public Guid Id { get; set; }

            public string Firstname { get; set; }

            public string Lastname { get; set; }

        }

        private ITestOutputHelper _outputHelper;
        private JsonPatchDocumentValidator<SuperHero> _validator;

        public JsonPatchDocumentValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _validator = new JsonPatchDocumentValidator<SuperHero>();
        }


        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
        }


        public static IEnumerable<object[]> CausesValidationErrorsCases
        {
            get
            {
                yield return new object[]
                {
                    new JsonPatchDocument<SuperHero>(),
                    ((Expression<Func<ValidationResult, bool>>)(
                        vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations) 
                            && x.Severity == Error)
                    )),
                    "Patch document has no operations"
                };

                {
                    JsonPatchDocument<SuperHero> patch = new JsonPatchDocument<SuperHero>();
                    patch.Replace(x => x.Lastname, "Grayson");
                    patch.Replace(x => x.Lastname, "Wayne");
                    yield return new object[]
                    {
                        patch,
                        ((Expression<Func<ValidationResult, bool>>)(
                            vr => !vr.IsValid
                                && vr.Errors.Count == 1
                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations) 
                                    && x.Severity == Error
                                    && x.ErrorMessage == $"Multiple operations on the same path with different values."
                                )
                        )),
                        "Patch document has multiple operations with different values for the same path"
                    };
                }
            } 
        }

        public static IEnumerable<object[]> CausesValidationWarningsCases
        {
            get
            {

                {
                    JsonPatchDocument<SuperHero> patch = new JsonPatchDocument<SuperHero>();
                    patch.Replace(x => x.Lastname, string.Empty);
                    patch.Replace(x => x.Lastname, string.Empty);
                    yield return new object[]
                    {
                        patch,
                        ((Expression<Func<ValidationResult, bool>>)(
                            vr => !vr.IsValid
                                && vr.Errors.Count == 1
                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
                                    && x.Severity == Error
                                    && x.ErrorMessage == $"Multiple operations on the same path with same values."
                                )
                        )),
                        "Patch document has multiple operations with same values for the same path"
                    };
                }

#if NETCOREAPP2_0
                {
                    JsonPatchDocument<SuperHero> patch = new JsonPatchDocument<SuperHero>();
                    patch.Replace(x => x.Lastname, string.Empty);
                    yield return new object[]
                    {
                        patch,
                        ((Expression<Func<ValidationResult, bool>>)(
                            vr => !vr.IsValid
                                && vr.Errors.Count == 1
                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
                                    && x.Severity == Warning
                                    && x.ErrorMessage == @"No ""test"" operation provided."
                                )
                        )),
                        @"Patch document has no ""test"" operation."
                    };
                }
#endif
            }
        }



        [Theory]
        [MemberData(nameof(CausesValidationErrorsCases))]
        public void ShouldFails_WithError(JsonPatchDocument<SuperHero> patchDocument, Expression<Func<ValidationResult, bool>> expectation, string because)
            => Test(patchDocument, expectation, because);

        [Theory]
        [MemberData(nameof(CausesValidationWarningsCases))]
        public void ShouldFails_WithWarning(JsonPatchDocument<SuperHero> patchDocument, Expression<Func<ValidationResult, bool>> expectation, string because)
            => Test(patchDocument, expectation, because);


        public void Test(JsonPatchDocument<SuperHero> patchDocument, Expression<Func<ValidationResult, bool>> expectation, string because)
        {
            _outputHelper.WriteLine($"{nameof(patchDocument)} : {SerializeObject(patchDocument)}");

            // Act
            ValidationResult vr = _validator.Validate(patchDocument);

            // Assert
            vr.Should().Match(expectation, because);
        }
    }
}

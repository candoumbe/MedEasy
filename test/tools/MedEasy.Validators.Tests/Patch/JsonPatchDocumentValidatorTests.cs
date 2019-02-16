using FluentAssertions;
using FluentValidation.Results;
using MedEasy.Validators.Patch;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using static Newtonsoft.Json.JsonConvert;
using static FluentValidation.Severity;
using Microsoft.AspNetCore.JsonPatch;
using Xunit.Categories;
#if NETCOREAPP2_0
using Microsoft.AspNetCore.JsonPatch.Operations;
#endif
namespace MedEasy.Validators.Tests.Patch
{
    [UnitTest]
    [Feature("Validation")]
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
            }
        }


        public static IEnumerable<object[]> ValidPatchDocumentsCases
        {
            get
            {
//                {
//                    JsonPatchDocument<SuperHero> patch = new JsonPatchDocument<SuperHero>();
//                    patch.Replace(x => x.Lastname, string.Empty);
//                    patch.Replace(x => x.Lastname, string.Empty);
//#if NETCOREAPP2_0
//                    patch.Test(x => x.Lastname, "newValue");
//#endif
//                    yield return new object[]
//                    {
//                        patch,
//#if NETCOREAPP2_0
//        ((Expression<Func<ValidationResult, bool>>)(
//                            vr => !vr.IsValid
//                                && vr.Errors.Count == 1
//                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
//                                    && x.Severity == Warning
//                                    && $@"Multiple operations on the same path : ""/{nameof(SuperHero.Lastname)}""".Equals(x.ErrorMessage)
//                                )
//                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
//                                    && x.Severity == Warning
//                                    && x.ErrorMessage == @"No ""test"" operation provided."
//                                )
//                        )),
//#else
//		((Expression<Func<ValidationResult, bool>>)(
//                            vr => !vr.IsValid
//                                && vr.Errors.Count == 1
//                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
//                                    && x.Severity == Warning
//                                    && $@"Multiple operations on the same path : ""/{nameof(SuperHero.Lastname).ToLower()}""".Equals(x.ErrorMessage)
//                                )
//                        )),
//#endif
//                        "Patch document has multiple operations with same values for the same path"
//                    };
//                }

//                {
//                    JsonPatchDocument<SuperHero> patch = new JsonPatchDocument<SuperHero>();
//                    patch.Replace(x => x.Lastname, string.Empty);
//                    patch.Replace(x => x.Lastname, string.Empty);
//                    patch.Replace(x => x.Firstname, null);
//                    patch.Replace(x => x.Firstname, string.Empty);
//#if NETCOREAPP2_0
//                    patch.Test(x => x.Lastname, "newValue");
//#endif

//                    yield return new object[]
//                    {
//                        patch,
//#if !NETCOREAPP2_0
//		((Expression<Func<ValidationResult, bool>>)(
//                            vr => !vr.IsValid
//                                && vr.Errors.Count == 2
//                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
//                                    && x.Severity == Warning
//                                    && $@"Multiple operations on the same path : ""/{nameof(SuperHero.Firstname).ToLower()}"", ""/{nameof(SuperHero.Lastname).ToLower()}""".Equals(x.ErrorMessage)
//                                )
//                        )),
//#else
//        ((Expression<Func<ValidationResult, bool>>)(
//                            vr => !vr.IsValid
//                                && vr.Errors.Count == 2
//                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
//                                    && x.Severity == Warning
//                                    && $@"Multiple operations on the same path : ""/{nameof(SuperHero.Firstname).ToLower()}"", ""/{nameof(SuperHero.Lastname).ToLower()}""".Equals(x.ErrorMessage)
//                                )
//                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
//                                    && x.Severity == Warning
//                                    && x.ErrorMessage == @"No ""test"" operation provided."
//                                )
//                        )),
//#endif
//                        "Patch document has multiple operations with same values for two differents paths"
//                    };
//                }

//#if NETCOREAPP2_0
//                {
//                    JsonPatchDocument<SuperHero> patch = new JsonPatchDocument<SuperHero>();
//                    patch.Replace(x => x.Lastname, string.Empty);

//                    yield return new object[]
//                    {
//                        patch,
//                        ((Expression<Func<ValidationResult, bool>>)(
//                            vr => !vr.IsValid
//                                && vr.Errors.Count == 1
//                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
//                                    && x.Severity == Warning
//                                    && x.ErrorMessage == @"No ""test"" operation provided."
//                                )
//                        )),
//                        @"Patch document has no ""test"" operation."
//                    };
//                }
//#endif
#if NETCOREAPP1_1
                {
                    JsonPatchDocument<SuperHero> patch = new JsonPatchDocument<SuperHero>();
                    patch.Replace(x => x.Lastname, string.Empty);

                    yield return new object[]
                    {
                        patch
                    };
                }
#elif NETCOREAPP2_0
                {
                    JsonPatchDocument<SuperHero> patch = new JsonPatchDocument<SuperHero>();
                    patch.Replace(x => x.Lastname, string.Empty);
                    patch.Test(x => x.Firstname, "Bruce" );

                    yield return new object[]
                    {
                        patch
                    };
                }

                {
                    JsonPatchDocument<SuperHero> patch = new JsonPatchDocument<SuperHero>();
                    patch.Replace(x => x.Lastname, string.Empty);
                    patch.Test(x => x.Lastname, "Bruce");

                    yield return new object[]
                    {
                        patch
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
        [MemberData(nameof(ValidPatchDocumentsCases))]
        public void Should_Be_Valid(JsonPatchDocument<SuperHero> patchDocument)
        {
            _outputHelper.WriteLine($"{nameof(patchDocument)} : {SerializeObject(patchDocument)}");

            // Act
            ValidationResult vr = _validator.Validate(patchDocument);

            // Assert
            vr.IsValid.Should().BeTrue();
        }

        private void Test(JsonPatchDocument<SuperHero> patchDocument, Expression<Func<ValidationResult, bool>> expectation, string because)
        {
            _outputHelper.WriteLine($"{nameof(patchDocument)} : {SerializeObject(patchDocument)}");

            // Act
            ValidationResult vr = _validator.Validate(patchDocument);

            // Assert
            vr.Should().Match(expectation, because);
        }
    }
}

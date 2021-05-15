#if NETCOREAPP2_0
using Microsoft.AspNetCore.JsonPatch.Operations;
#endif
namespace MedEasy.Validators.Tests.Patch
{
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

        public static IEnumerable<object[]> InvalidPatchDocumentsCases
        {
            get
            {
                yield return new object[]
                {
                    new JsonPatchDocument<SuperHero>(),
                    (Expression<Func<ValidationResult, bool>>)(
                        vr => !vr.IsValid
                            && vr.Errors.Exactly(1)
                            && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
                            && x.Severity == Error)
                    ),
                    "Patch document has no operations"
                };
                {
                    JsonPatchDocument<SuperHero> patch = new();
                    patch.Replace(x => x.Lastname, string.Empty);
                    patch.Replace(x => x.Lastname, string.Empty);

                    patch.Test(x => x.Lastname, "newValue");

                    yield return new object[]
                    {
                        patch,

                        (Expression<Func<ValidationResult, bool>>)(
                            vr => !vr.IsValid
                                && vr.Errors.Exactly(1)
                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
                                    && x.Severity == Warning
                                    && $@"Multiple operations on the same path : ""/{nameof(SuperHero.Lastname)}""".Equals(x.ErrorMessage))
                        ),
                        "Patch document has multiple operations with same values for the same path"
                    };
                }

                {
                    JsonPatchDocument<SuperHero> patch = new();
                    patch.Replace(x => x.Lastname, string.Empty);
                    patch.Replace(x => x.Lastname, string.Empty);
                    patch.Replace(x => x.Firstname, null);
                    patch.Replace(x => x.Firstname, string.Empty);
                    patch.Test(x => x.Lastname, "newValue");

                    yield return new object[]
                    {
                        patch,
                        (Expression<Func<ValidationResult, bool>>)(
                            vr => !vr.IsValid
                                && vr.Errors.Once()
                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
                                    && x.Severity == Warning
                                    && $@"Multiple operations on the same path : ""/{nameof(SuperHero.Firstname).ToLower()}"", ""/{nameof(SuperHero.Lastname).ToLower()}""".Equals(x.ErrorMessage, StringComparison.OrdinalIgnoreCase)
                                )
                        ),
                        "Patch document has multiple operations with different values for two differents paths"
                    };
                }

                {
                    JsonPatchDocument<SuperHero> patch = new();
                    patch.Replace(x => x.Lastname, string.Empty);

                    yield return new object[]
                    {
                        patch,
                        (Expression<Func<ValidationResult, bool>>)(
                            vr => !vr.IsValid
                                && vr.Errors.Exactly(1)
                                && vr.Errors.Once(x => x.PropertyName == nameof(JsonPatchDocument<SuperHero>.Operations)
                                    && x.Severity == Warning
                                    && x.ErrorMessage == @"No ""test"" operation provided."
                                )
                        ),
                        @"Patch document has no ""test"" operation."
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(InvalidPatchDocumentsCases))]
        public void ShouldFails_WithError(JsonPatchDocument<SuperHero> patchDocument, Expression<Func<ValidationResult, bool>> expectation, string because)
            => Test(patchDocument, expectation, because);

        private void Test(JsonPatchDocument<SuperHero> patchDocument, Expression<Func<ValidationResult, bool>> expectation, string because)
        {
            _outputHelper.WriteLine($"{nameof(patchDocument)} : {SerializeObject(patchDocument)}");

            // Act
            ValidationResult vr = _validator.Validate(patchDocument);

            // Assert
            _outputHelper.WriteLine($"Errors : {vr.Errors.Jsonify()}");
            vr.Should().Match(expectation, because);
        }
    }
}

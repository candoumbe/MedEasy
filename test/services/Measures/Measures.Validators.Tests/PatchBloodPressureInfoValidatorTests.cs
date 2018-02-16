using FluentAssertions;
using FluentValidation.Results;
using Measures.DTO;
using Measures.Objects;
using MedEasy.DAL.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FluentValidation.Severity;
using static Newtonsoft.Json.JsonConvert;
using static Moq.MockBehavior;
using static System.StringComparison;
namespace Measures.Validators.Tests
{
    public class PatchBloodPressureInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private PatchBloodPressureInfoValidator _validator;

        public PatchBloodPressureInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _validator = new PatchBloodPressureInfoValidator(_unitOfWorkFactoryMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
            _unitOfWorkFactoryMock = null;
        }

        [Fact]
        public void ThrowsArgumentNullException()
        {
            // Act
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new PatchBloodPressureInfoValidator(null);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();


        }


        public static IEnumerable<object[]> InvalidPatchDocumentCases
        {
            get
            {
                yield return new object[] 
                {
                    new JsonPatchDocument<BloodPressureInfo>(),
                    ((Expression<Func<ValidationResult, bool>>) (vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(error => "Operations".Equals(error.PropertyName, OrdinalIgnoreCase) && error.Severity == Error))),
                    "Patch document has no operation."
                };

                {
                    JsonPatchDocument<BloodPressureInfo> patchDocument = new JsonPatchDocument<BloodPressureInfo>();
                    patchDocument.Replace(x => x.Id, default);

                    yield return new object[]
                    {
                        patchDocument,
                        ((Expression<Func<ValidationResult, bool>>) (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(error => "Operations".Equals(error.PropertyName, OrdinalIgnoreCase) && error.Severity == Error))),
                        $"patch docment cannot contains any 'replace' operation on '/{nameof(BloodPressureInfo.Id)}'."
                    };
                }

                {
                    JsonPatchDocument<BloodPressureInfo> patchDocument = new JsonPatchDocument<BloodPressureInfo>();
                    patchDocument.Remove(x => x.Id);

                    yield return new object[]
                    {
                        patchDocument,
                        ((Expression<Func<ValidationResult, bool>>) (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(error => "Operations".Equals(error.PropertyName, OrdinalIgnoreCase) && error.Severity == Error))),
                        $"patch docment cannot contains any 'remove' operation on '/{nameof(BloodPressureInfo.Id)}'."
                    };
                }

                {
                    JsonPatchDocument<BloodPressureInfo> patchDocument = new JsonPatchDocument<BloodPressureInfo>();
                    patchDocument.Add(x => x.Id, Guid.NewGuid());

                    yield return new object[]
                    {
                        patchDocument,
                        ((Expression<Func<ValidationResult, bool>>) (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(error => "Operations".Equals(error.PropertyName, OrdinalIgnoreCase) && error.Severity == Error))),
                        $"patch docment cannot contains any 'add' operation on '/{nameof(BloodPressureInfo.Id)}'."
                    };
                }
            }
        }


        [Theory]
        [MemberData(nameof(InvalidPatchDocumentCases))]
        [Trait("Category", "Unit test")]
        public async Task Validate(JsonPatchDocument<BloodPressureInfo> changes, Expression<Func<ValidationResult, bool>> expectation, string reason)
        {
            
            // Act
            _outputHelper.WriteLine($"Input : {SerializeObject(changes)}");
            ValidationResult vr = await _validator.ValidateAsync(changes)
                .ConfigureAwait(false);


            // Assert
            vr.Should().Match(expectation, reason);
        }
    }
}

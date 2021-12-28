namespace Measures.Validators.Tests.Features.Patients
{
    using FluentAssertions;

    using FluentValidation.Results;

    using Measures.DTO;
    using Measures.Ids;
    using Measures.Validators.Commands.Patients;

    using MedEasy.DAL.Interfaces;

    using Microsoft.AspNetCore.JsonPatch;

    using Moq;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static FluentValidation.Severity;
    using static Moq.MockBehavior;
    using static Newtonsoft.Json.JsonConvert;
    using static System.StringComparison;

    [UnitTest]
    public class PatchPatientInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private PatchPatientInfoValidator _validator;

        public PatchPatientInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.NewUnitOfWork().Dispose());

            _validator = new PatchPatientInfoValidator(_unitOfWorkFactoryMock.Object);
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
            Action action = () => new PatchPatientInfoValidator(null);
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
                    new JsonPatchDocument<SubjectInfo>(),
                    (Expression<Func<ValidationResult, bool>>) (vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(error => "Operations".Equals(error.PropertyName, OrdinalIgnoreCase) && error.Severity == Error)),
                    "Patch document has no operation."
                };

                {
                    JsonPatchDocument<SubjectInfo> patchDocument = new();
                    patchDocument.Replace(x => x.Id, default);

                    yield return new object[]
                    {
                        patchDocument,
                        (Expression<Func<ValidationResult, bool>>) (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(error => "Operations".Equals(error.PropertyName, OrdinalIgnoreCase) && error.Severity == Error)),
                        $"patch docment cannot contains any 'replace' operation on '/{nameof(SubjectInfo.Id)}'."
                    };
                }

                {
                    JsonPatchDocument<SubjectInfo> patchDocument = new();
                    patchDocument.Remove(x => x.Id);

                    yield return new object[]
                    {
                        patchDocument,
                        (Expression<Func<ValidationResult, bool>>) (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(error => "Operations".Equals(error.PropertyName, OrdinalIgnoreCase) && error.Severity == Error)),
                        $"patch docment cannot contains any 'remove' operation on '/{nameof(SubjectInfo.Id)}'."
                    };
                }

                {
                    JsonPatchDocument<SubjectInfo> patchDocument = new();
                    patchDocument.Add(x => x.Id, SubjectId.New());

                    yield return new object[]
                    {
                        patchDocument,
                        (Expression<Func<ValidationResult, bool>>) (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(error => "Operations".Equals(error.PropertyName, OrdinalIgnoreCase) && error.Severity == Error)),
                        $"patch docment cannot contains any 'add' operation on '/{nameof(SubjectInfo.Id)}'."
                    };
                }

                string computeLongString(int desiredLength)
                {
                    StringBuilder sb = new();

                    while (sb.Length < desiredLength)
                    {
                        sb.Append(Path.GetRandomFileName().Replace(".", string.Empty));
                    }

                    return sb.ToString(0, desiredLength);
                }
                {
                    string nameIsTooLong = computeLongString(101);
                    JsonPatchDocument<SubjectInfo> patchDocument = new();
                    patchDocument.Replace(x => x.Name, nameIsTooLong);

                    yield return new object[]
                    {
                        patchDocument,
                        (Expression<Func<ValidationResult, bool>>) (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(error => "Operations".Equals(error.PropertyName, OrdinalIgnoreCase)
                                && error.Severity == Error)),
                        $"new {nameof(SubjectInfo.Name)} cannot contain more than 100 characters."
                    };
                }
                {
                    string nameIsTooLong = computeLongString(100);
                    JsonPatchDocument<SubjectInfo> patchDocument = new();
                    patchDocument.Replace(x => x.Name, nameIsTooLong);

                    yield return new object[]
                    {
                        patchDocument,
                        (Expression<Func<ValidationResult, bool>>) (vr => vr.IsValid),
                        $"new {nameof(SubjectInfo.Name)} is 100 characters long."
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(InvalidPatchDocumentCases))]
        public async Task Validate(JsonPatchDocument<SubjectInfo> changes, Expression<Func<ValidationResult, bool>> expectation, string reason)
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

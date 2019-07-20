using FluentAssertions;
using FluentValidation.Results;
using MedEasy.DTO.Search;
using Patients.DTO;
using Patients.Validators.Features.Patients.Queries;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static FluentValidation.Severity;
using static Newtonsoft.Json.JsonConvert;

namespace Patients.Validators.Tests.Search
{
    [UnitTest]
    [Feature("Measures")]
    public class SearchPatientInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private SearchPatientInfoValidator _validator;

        public SearchPatientInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _validator = new SearchPatientInfoValidator();
        }

        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
        }

        public static IEnumerable<object[]> ValidateCases
        {
            get
            {
                yield return new object[] {
                    new SearchPatientInfo(),
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 4
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Firstname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Lastname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.BirthDate).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Sort).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because {nameof(SearchPatientInfo.Firstname)}/{nameof(SearchPatientInfo.Lastname)}/{nameof(SearchPatientInfo.BirthDate)}/{nameof(SearchPatientInfo.Sort)} not set."
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Sort = "-Name"},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Sort).Equals(errorItem.PropertyName)
                                && errorItem.Severity == Error
                                && errorItem.ErrorMessage == "Unknown <Name> property."
                            )
                    )),
                    $"Sorting by a property that is not either {nameof(SearchPatientInfo.Firstname)}/{nameof(SearchPatientInfo.Lastname)}/{nameof(SearchPatientInfo.BirthDate)}"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Firstname = "Hugo", Sort="Firstname,Name,Nickname"},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Sort).Equals(errorItem.PropertyName)
                                && errorItem.Severity == Error
                                && errorItem.ErrorMessage == "Unknown <Name, Nickname> properties."
                            )
                    )),
                    $"Sorting by a property that is not either {nameof(SearchPatientInfo.Firstname)}/{nameof(SearchPatientInfo.Lastname)}/{nameof(SearchPatientInfo.BirthDate)}"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Firstname = "Hugo", Sort=$"{nameof(SearchPatientInfo.Firstname)}", Page = -1},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Page).Equals(errorItem.PropertyName)
                                && errorItem.Severity == Error
                            )
                    )),
                    $"{nameof(SearchPatientInfo.Page)} < 1"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Firstname = "Hugo", Sort=$"{nameof(SearchPatientInfo.Firstname)}", Page = 1, PageSize = -1},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.PageSize).Equals(errorItem.PropertyName)
                                && errorItem.Severity == Error
                            )
                    )),
                    $"{nameof(SearchPatientInfo.PageSize)} < 1"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Firstname = "Hugo", Sort=$"--{nameof(SearchPatientInfo.Firstname)}", Page = 1, PageSize = 20},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Sort).Equals(errorItem.PropertyName)
                                && errorItem.Severity == Error
                            )
                    )),
                    $@"""--{nameof(SearchPatientInfo.Firstname)}"" contains two consecutive hyphens"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Firstname = "Hugo", Sort=$"-{nameof(SearchPatientInfo.Firstname)},--{nameof(SearchPatientInfo.Lastname)}", Page = 1, PageSize = 20},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Sort).Equals(errorItem.PropertyName)
                                && errorItem.Severity == Error
                                && errorItem.ErrorMessage == $@"Sort expression ""--Lastname"" does not match ""{AbstractSearchInfo<PatientInfo>.SortPattern}""."
                        )
                    )),
                    $@"""--Lastname"" contains two or more consecutive hyphens"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Firstname = "Hugo", Sort=$"--{nameof(SearchPatientInfo.Lastname)}, --{nameof(SearchPatientInfo.Firstname)}", Page = 1, PageSize = 20},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Sort).Equals(errorItem.PropertyName)
                                && errorItem.Severity == Error
                                && errorItem.ErrorMessage == $@"Sort expressions ""--Lastname"", ""--Firstname"" do not match ""{AbstractSearchInfo<PatientInfo>.SortPattern}""."
                        )
                    )),
                    $"{nameof(SearchPatientInfo.Sort)} contains a sort expression with two or more consecutive hyphens"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Sort = " Firstname"},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => vr.IsValid)),
                    $"Sorting by a property with the same case"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Sort = " firstname"},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => vr.IsValid)),
                    $"Sorting by a property with different case"
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidateCases))]
        public async Task TestValidate(SearchPatientInfo search, Expression<Func<ValidationResult, bool>> expectation, string reason)
        {
            _outputHelper.WriteLine($"search : {SerializeObject(search)}");

            // Act
            ValidationResult result = await _validator.ValidateAsync(search)
                .ConfigureAwait(false);

            // Assert
            result.Should()
                .Match(expectation, reason);
        }

    }
}

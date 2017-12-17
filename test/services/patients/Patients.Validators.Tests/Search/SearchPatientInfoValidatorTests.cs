﻿using FluentAssertions;
using FluentValidation.Results;
using Patients.DTO;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FluentValidation.Severity;
using static Newtonsoft.Json.JsonConvert;

namespace Patients.Validators.Tests.Search
{
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

        public static IEnumerable<object[]> InvalidCases
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
                                && errorItem.ErrorMessage == "Cannot sort by unknown <Name> property."
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
                                && errorItem.ErrorMessage == "Cannot sort by unknown <Name,Nickname> properties."
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
                    $"{nameof(SearchPatientInfo.Page)}'s value must be equal to or greater than 1"
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
                    $"{nameof(SearchPatientInfo.PageSize)}'s value must be equal to or greater than 1"
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
                    $"{nameof(SearchPatientInfo.Sort)}'s value contains two or more consecutive hyphens"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Firstname = "Hugo", Sort=$"-{nameof(SearchPatientInfo.Firstname)},--{nameof(SearchPatientInfo.Lastname)}", Page = 1, PageSize = 20},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Sort).Equals(errorItem.PropertyName)
                                && errorItem.Severity == Error
                                && errorItem.ErrorMessage == $@"Sort expression ""--Lastname"" does not match ""{SearchPatientInfoValidator.SortPattern}""."
                        )
                    )),
                    $"{nameof(SearchPatientInfo.Sort)} contains a sort expression with two or more consecutive hyphens"
                };

                yield return new object[]
                {
                    new SearchPatientInfo { Firstname = "Hugo", Sort=$"--{nameof(SearchPatientInfo.Lastname)}, --{nameof(SearchPatientInfo.Firstname)}", Page = 1, PageSize = 20},
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(SearchPatientInfo.Sort).Equals(errorItem.PropertyName)
                                && errorItem.Severity == Error
                                && errorItem.ErrorMessage == $@"Sort expressions ""--Lastname"", ""--Firstname"" do not match ""{SearchPatientInfoValidator.SortPattern}""."
                        )
                    )),
                    $"{nameof(SearchPatientInfo.Sort)} contains a sort expression with two or more consecutive hyphens"
                };
            }

        }

        [Theory]
        [MemberData(nameof(InvalidCases))]
        public async Task Should_Not_Be_Valid(SearchPatientInfo search, Expression<Func<ValidationResult, bool>> validationResultExpectation, string reason)
        {
            _outputHelper.WriteLine($"search : {SerializeObject(search)}");

            // Act
            ValidationResult result = await _validator.ValidateAsync(search)
                .ConfigureAwait(false);

            // Assert
            result.Should().Match(validationResultExpectation, reason);

        }



    }
}
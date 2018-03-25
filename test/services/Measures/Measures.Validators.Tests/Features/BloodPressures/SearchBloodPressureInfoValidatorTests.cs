using FluentAssertions;
using FluentAssertions.Extensions;
using FluentValidation.Results;
using Measures.DTO;
using Measures.Validators.Queries.BloodPressures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FluentValidation.Severity;
using static Newtonsoft.Json.JsonConvert;

namespace Measures.Validators.Tests.Features.Queries.BloodPressures
{
    public class SearchBloodPressureInfoValidatorTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private SearchBloodPressureInfoValidator _sut;

        public SearchBloodPressureInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _sut = new SearchBloodPressureInfoValidator();
        }

        public void Dispose() => _sut = null;

        public static IEnumerable<object[]> ValidateSearchCases
        {
            get
            {
                yield return new object[]
                {
                    new SearchBloodPressureInfo(),
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Count == 3
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.From))
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.To))
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.Sort))   
                    )),
                    "no property set"

                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { From = 23.July(2010) },
                    ((Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid)),
                    $"{nameof(SearchBloodPressureInfo.From)} is not null"

                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { Sort = string.Empty },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Count() == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.Sort) && err.Severity == Error)
                    )),
                    $"{nameof(SearchBloodPressureInfo.Sort)} is set to an empty string"

                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { Sort = "UnknownProperty" },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Count() == 1 
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.Sort) 
                            && err.Severity == Error
                            && err.ErrorMessage == "Unknown <UnknownProperty> property."
                        )
                    )),
                    $"{nameof(SearchBloodPressureInfo.Sort)} is set sort by a property that is not a {nameof(BloodPressureInfo)} property"

                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { Sort = "UnknownProperty1, UnknownProperty2" },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Count() == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.Sort)
                            && err.Severity == Error
                            && err.ErrorMessage == "Unknown <UnknownProperty1, UnknownProperty2> properties."
                        )
                    )),
                    $"{nameof(SearchBloodPressureInfo.Sort)} is set with a value that doesn't contain any of {nameof(BloodPressureInfo)} properties"
                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { Sort = $"UnknownProperty1, {nameof(BloodPressureInfo.DiastolicPressure)}" },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Count() == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.Sort)
                            && err.Severity == Error
                            && err.ErrorMessage == "Unknown <UnknownProperty1> property."
                        )
                    )),
                    $"{nameof(SearchBloodPressureInfo.Sort)} is set with a value which contains some fields that are not names of {nameof(BloodPressureInfo)} properties"
                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { Sort = $"-{nameof(BloodPressureInfo.DiastolicPressure)}" },
                    ((Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid)),
                    $"{nameof(SearchBloodPressureInfo.Sort)} is set to order by {nameof(BloodPressureInfo.DiastolicPressure)} in descending order"
                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { Sort = $"--{nameof(BloodPressureInfo.DiastolicPressure)}" },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Count() == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.Sort) 
                            && err.Severity == Error )
                    )),
                    $"{nameof(SearchBloodPressureInfo.Sort)} value cannot contain any field that start with two consecutives hyphens {nameof(BloodPressureInfo.DiastolicPressure)} in descending order"
                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { From = 1.February(2000), To = 1.January(2000) },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Count() == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.From)
                            && err.Severity == Error )
                    )),
                    $"{nameof(SearchBloodPressureInfo.From)} value cannot be greater than {nameof(SearchBloodPressureInfo.To)} value."
                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { PatientId = Guid.NewGuid() },
                    ((Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid )),
                    $"{nameof(SearchBloodPressureInfo.PatientId)} is set."
                };

                yield return new object[]
                {
                    new SearchBloodPressureInfo { Page = -1, PatientId = Guid.NewGuid() },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Count() == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchBloodPressureInfo.Page)
                            && err.Severity == Error )
                    )),
                    $"{nameof(SearchBloodPressureInfo.Page)} is negative."
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidateSearchCases))]
        public async Task ValidateSearch(SearchBloodPressureInfo search, Expression<Func<ValidationResult, bool>> validationResultExpectation, string reason)
        {
            _outputHelper.WriteLine($"Search : {SerializeObject(search, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented })}");

            // Act
            ValidationResult vr = await _sut.ValidateAsync(search)
                .ConfigureAwait(false);

            // Assert
            vr.Should()
                .Match(validationResultExpectation, reason);
        }

    }
}

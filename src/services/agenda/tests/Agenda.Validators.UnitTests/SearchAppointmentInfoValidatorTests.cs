using Agenda.DTO;
using Agenda.DTO.Resources.Search;

using FluentAssertions;
using FluentAssertions.Extensions;

using FluentValidation.Results;

using MedEasy.DTO.Search;

using NodaTime.Extensions;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

using static FluentValidation.Severity;

namespace Agenda.Validators.UnitTests
{
    [Feature("Agenda")]
    [UnitTest]
    public class SearchAppointmentInfoValidatorTests
    {
        private static ITestOutputHelper _outputHelper;
        private readonly SearchAppointmentInfoValidator _sut;

        public SearchAppointmentInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _sut = new SearchAppointmentInfoValidator();
        }

        public static IEnumerable<object[]> ValidateCases
        {
            get
            {
                yield return new object[]
                {
                    new SearchAppointmentInfo(),
                    (Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid),
                    "no property set"
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { Page = -1 },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchAppointmentInfo.Page) && err.Severity == Error)
                    ),
                    $"{nameof(SearchAppointmentInfo.Page)} is negative"
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { PageSize = -1 },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchAppointmentInfo.PageSize) && err.Severity == Error)
                    ),
                    $"{nameof(SearchAppointmentInfo.PageSize)} is negative"
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { From = 1.February(2010).AsUtc().ToInstant().InUtc(), To = 1.January(2010).AsUtc().ToInstant().InUtc()  },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchAppointmentInfo.To) && err.Severity == Error)
                    ),
                    $"{nameof(SearchAppointmentInfo.From)} >= {nameof(SearchAppointmentInfo.To)}"
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { From = 1.February(2010).AsUtc().ToInstant().InUtc() },
                    (Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid
                    ),
                    $"only {nameof(SearchAppointmentInfo.From)} was explicitely set"
                };
                yield return new object[]
                {
                    new SearchAppointmentInfo { Sort = "+startDate" },
                    (Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid
                    ),
                    $"only {nameof(SearchAppointmentInfo.Sort)} was explicitely set"
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { To = 1.February(2010).AsUtc().ToInstant().InUtc() },
                    (Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid
                    ),
                    $"only {nameof(SearchAppointmentInfo.To)} was explicitely set"
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { Sort = "-UnknownProp" },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchAppointmentInfo.Sort) && err.Severity == Error
                            && "Unknown <UnknownProp> property.".Equals(err.ErrorMessage))
                    ),
                    $@"Sort value contains an unknown sort clause ""-UnknownProp"""
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { Sort = "+UnknownProp" },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchAppointmentInfo.Sort) && err.Severity == Error
                            && "Unknown <UnknownProp> property.".Equals(err.ErrorMessage))
                    ),
                    $@"Sort value contains an unknown sort clause ""+UnknownProp"""
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { Sort = $"{nameof(AppointmentInfo.EndDate)},+UnknownProp" },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchAppointmentInfo.Sort) && err.Severity == Error
                            && "Unknown <UnknownProp> property.".Equals(err.ErrorMessage))
                    ),
                    $@"Sort value contains an unknown sort clause ""+UnknownProp"""
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { Sort = $"{nameof(AppointmentInfo.EndDate).ToLowerInvariant()}" },
                    (Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid),
                    $@"Sort properties are not case sensitive"
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { Sort = $"++{nameof(AppointmentInfo.EndDate)}" },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchAppointmentInfo.Sort) && err.Severity == Error
                            && $@"Sort expression ""++{nameof(AppointmentInfo.EndDate)}"" does not match ""{AbstractSearchInfo<AppointmentInfo>.SortPattern}"".".Equals(err.ErrorMessage))
                    ),
                    $@"Sort expression ""++{nameof(AppointmentInfo.EndDate)}"" contains two ""++"""
                };

                yield return new object[]
                {
                    new SearchAppointmentInfo { Sort = $"--{nameof(AppointmentInfo.EndDate)}" },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(SearchAppointmentInfo.Sort) && err.Severity == Error
                            && $@"Sort expression ""--{nameof(AppointmentInfo.EndDate)}"" does not match ""{AbstractSearchInfo<AppointmentInfo>.SortPattern}"".".Equals(err.ErrorMessage))
                    ),
                    $@"Sort expression ""--{nameof(AppointmentInfo.EndDate)}"" contains two ""--"""
                };

            }
        }

        [Theory]
        [MemberData(nameof(ValidateCases))]
        public async Task ValidateSearchAppointmentInfo(SearchAppointmentInfo search, Expression<Func<ValidationResult, bool>> validationResultExpectation, string reason)
        {
            _outputHelper.WriteLine($"criteria : {search.Jsonify()}");
            // Arrange

            // Act
            ValidationResult vr = await _sut.ValidateAsync(search)
                .ConfigureAwait(false);

            // Assert
            vr.Should()
                .Match(validationResultExpectation, reason);
        }
    }
}

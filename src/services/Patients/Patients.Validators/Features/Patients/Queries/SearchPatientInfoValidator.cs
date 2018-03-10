using FluentValidation;
using Patients.DTO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using static FluentValidation.Severity;
using static System.StringSplitOptions;

namespace Patients.Validators.Features.Patients.Queries
{
    /// <summary>
    /// Validates <see cref="SearchPatientInfo"/> instances.
    /// </summary>
    public class SearchPatientInfoValidator : AbstractValidator<SearchPatientInfo>
    {

        /// <summary>
        /// List of properties that can be used to sort results found using a <see cref="SearchPatientInfo"/>
        /// </summary>
        private static string[] _sortableProperties = new[] {
        nameof(SearchPatientInfo.BirthDate),
        nameof(SearchPatientInfo.Firstname),
        nameof(SearchPatientInfo.Lastname),
    };

        /// <summary>
        /// Pattern of a sort expression to apply on a property
        /// </summary>
        public const string SortPattern = @"^(\s*(-?\w)+)(,\s*(-?\w)+)*";

        private static Regex _sortExpressionRegex = new Regex(SortPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

        private static Func<string, string[]> _extractPropertyNamesFromSortQuery = (sort) => sort.Split(new[] { "," }, RemoveEmptyEntries)
            .Select(x => x.Replace("-", string.Empty).Trim())
            .ToArray()
        ;

        public SearchPatientInfoValidator()
        {
            When(x => string.IsNullOrWhiteSpace(x.Firstname)
                    && string.IsNullOrWhiteSpace(x.Lastname)
                    && x.BirthDate == null
                    && string.IsNullOrWhiteSpace(x.Sort),
                () =>
                {
                    RuleFor(x => x.Firstname).NotEmpty();
                    RuleFor(x => x.Lastname).NotEmpty();
                    RuleFor(x => x.BirthDate).NotEmpty();
                    RuleFor(x => x.Sort).NotEmpty();
                });


            When(x => !string.IsNullOrWhiteSpace(x.Sort),
                () =>
                {
                    RuleFor(x => x.Sort)
                        .Must(sort => !sort.Split(new[] { ',' }).Any(x => string.IsNullOrWhiteSpace(x)))
                        .WithSeverity(Error)
                        .WithMessage("Cannot contains empty expressions");

                    RuleFor(x => x.Sort)
                        .Must(sort =>
                            sort.Split(new[] { ',' })
                                .AsParallel()
                                .All(x => _sortExpressionRegex.IsMatch(x))
                        )
                        .WithMessage(search =>
                        {
                            string[] incorrectExpresions = search.Sort.Split(new[] { ',' }, RemoveEmptyEntries)
                                .Where(x => !_sortExpressionRegex.IsMatch(x))
                                .Select(x => $@"""{x.Trim()}""")
                                .ToArray();

                            return $"Sort expression{(incorrectExpresions.Length == 1 ? string.Empty : "s")} {string.Join(", ", incorrectExpresions)} " +
                            $@"do{(incorrectExpresions.Length == 1 ? "es" : string.Empty)} not match ""{SortPattern}"".";
                        });

                    RuleFor(x => x.Sort)
                        .Must((sort) => !_extractPropertyNamesFromSortQuery(sort)
                                .Except(_sortableProperties)
                                .Any()
                        )
                        .WithSeverity(Error)
                        .WithMessage(search =>
                        {
                            string[] unknownProperties = _extractPropertyNamesFromSortQuery(search.Sort)
                                .Except(_sortableProperties)
                                .ToArray();
                            return $"Cannot sort by unknown <{string.Join(",", unknownProperties)}> propert{(unknownProperties.Length == 1 ? "y" : "ies")}.";
                        })
                        ;

                });

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithSeverity(Error);

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithSeverity(Error);

        }
    } 
}
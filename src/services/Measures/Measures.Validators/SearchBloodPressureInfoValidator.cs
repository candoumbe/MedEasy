using FluentValidation;
using Measures.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static FluentValidation.Severity;

namespace Measures.Validators
{
    /// <summary>
    /// Validates <see cref="SearchBloodPressureInfo"/> instances.
    /// </summary>
    public class SearchBloodPressureInfoValidator : AbstractValidator<SearchBloodPressureInfo>
    {
        private static Regex _sortRegex = new Regex(SearchBloodPressureInfo.SortPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

        /// <summary>
        /// Builds a new <see cref="SearchBloodPressureInfoValidator"/> instance.
        /// </summary>
        public SearchBloodPressureInfoValidator()
        {

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.From)
                .NotNull()
                .Unless(x => x.To != default || x.Sort != default);

            RuleFor(x => x.To)
                .NotNull()
                .Unless(x => x.From != default || x.Sort != default);

            RuleFor(x => x.Sort)
                .NotEmpty()
                .Unless(x => x.From.HasValue || x.To.HasValue);


            When(
                (x) => !string.IsNullOrWhiteSpace(x.Sort),
                () =>
                {
                    IEnumerable<string> sortablesProperties = typeof(BloodPressureInfo).GetTypeInfo()
                        .GetRuntimeProperties()
                        .AsParallel()
                        .Where(pi => pi.CanRead)
                        .Select(pi => pi.Name);

                    IEnumerable<string> extractUnknownPropertiesNames(string input)
                    {

                        IEnumerable<string> sortFields = input.Split(new string[] { SearchBloodPressureInfo.SortSeparator }, StringSplitOptions.None)
                                .Select(fieldName => {

                                    string sanitizedFieldName = fieldName.StartsWith("-") || fieldName.StartsWith("+") || fieldName.StartsWith("!")
                                        ? fieldName.Substring(1)
                                        : fieldName;

                                    return sanitizedFieldName.Trim();
                                });
                        IEnumerable<string> unknownProperties = sortFields.Except(sortablesProperties)
                            .ToArray();

                        return unknownProperties;
                    }


                    RuleFor(x => x.Sort)
                        .Matches(_sortRegex)
                        .Must((string sort) =>
                        {
                            IEnumerable<string> unknownProperties = extractUnknownPropertiesNames(sort);

                            return !unknownProperties.Any();
                        })
                        .WithSeverity(Error)
                        .WithMessage((x) =>
                        {
                            IEnumerable<string> unknownProperties = extractUnknownPropertiesNames(x.Sort);
                            return $"Unknown <{string.Join(", ", unknownProperties)}> propert{(unknownProperties.Count() == 1 ? "y" : "ies")}.";
                        });


                }
            );

            When(
                (x) => x.From.HasValue && x.To.HasValue,
                () =>
                {
                    RuleFor(x => x.From)
                        .LessThanOrEqualTo(x => x.To);
                }
            );

        }
    }
}

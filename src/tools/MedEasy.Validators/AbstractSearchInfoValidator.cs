using FluentValidation;

using MedEasy.DTO.Search;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using static FluentValidation.Severity;
using static System.StringSplitOptions;

namespace MedEasy.Validators.Validators
{
    /// <summary>
    /// Validator for <see cref="AbstractSearchInfo{T}"/> instances
    /// </summary>
    /// <typeparam name="T">Type constraint of the <see cref="AbstractSearchInfo{T}"/></typeparam>
    public class AbstractSearchInfoValidator<T> : AbstractValidator<AbstractSearchInfo<T>>
    {
        private static Regex _sortRegex => new(AbstractSearchInfo<T>.SortPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

        public AbstractSearchInfoValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1);

            When(
                (x) => !string.IsNullOrWhiteSpace(x.Sort),
                () =>
                {
                    IEnumerable<string> sortablesProperties = typeof(T).GetTypeInfo()
                        .GetRuntimeProperties()
                        .AsParallel()
                        .Where(pi => pi.CanRead)
                        .Select(pi => pi.Name);

                    IEnumerable<string> extractUnknownPropertiesNames(IEnumerable<string> fields)
                    {
                        IEnumerable<string> sortFields = fields
                                .Select(fieldName =>
                                {
                                    string sanitizedFieldName = Regex.Replace(fieldName, @"(-|\+|\!)+", string.Empty);
                                    return sanitizedFieldName.Trim();
                                });
                        IEnumerable<string> unknownProperties = sortFields.Except(sortablesProperties, StringComparer.OrdinalIgnoreCase)
                            .ToArray();

                        return unknownProperties;
                    }

                    RuleFor(x => x.Sort)
                        .Matches(_sortRegex)
                        .WithMessage(search =>
                        {
                            string[] incorrectExpresions = search.Sort.Split(new[] { AbstractSearchInfo<T>.SortSeparator }, RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .Where(x => !_sortRegex.IsMatch(x))
                                .Select(x => $@"""{x}""")
                                .ToArray();

                            return $"Sort expression{(incorrectExpresions.Length == 1 ? string.Empty : "s")} {string.Join(", ", incorrectExpresions)} " +
                            $@"do{(incorrectExpresions.Length == 1 ? "es" : string.Empty)} not match ""{AbstractSearchInfo<T>.SortPattern}"".";
                        })
                        .Must((string sort) =>
                        {
                            IEnumerable<string> fields = sort.Split(new[] { AbstractSearchInfo<T>.SortSeparator }, RemoveEmptyEntries);

                            IEnumerable<string> unknownProperties = extractUnknownPropertiesNames(fields);
                            return !unknownProperties.Any();
                        })
                        .WithSeverity(Error)
                        .WithMessage((x) =>
                        {
                            IEnumerable<string> fields = x.Sort.Split(new[] { AbstractSearchInfo<T>.SortSeparator }, RemoveEmptyEntries)
                                .Where(field => _sortRegex.IsMatch(field));
                            IEnumerable<string> unknownProperties = extractUnknownPropertiesNames(fields);
                            return $"Unknown <{string.Join(", ", unknownProperties)}> propert{(unknownProperties.Count() == 1 ? "y" : "ies")}.";
                        })
                        ;
                }
            );
        }
    }
}

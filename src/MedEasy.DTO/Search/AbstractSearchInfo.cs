using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Base class
    /// </summary>
    /// <typeparam name="T">Type of the searched resources.</typeparam>
    public abstract class AbstractSearchInfo<T> : IValidatableObject
    {

        public const string SortPattern = @"\s*(-{0,1}_*[a-zA-Z]+){0,1}\s*";

        /// <summary>
        /// Index of the page of result to read.
        /// </summary>
        /// <remarks>
        /// The first page 
        /// </remarks>
        public int Page { get; set; }

        /// <summary>
        /// Size of a page 
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Sorts
        /// </summary>
        [RegularExpression(SortPattern)]
        public string Sort { get; set; }


        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            

            if (!string.IsNullOrWhiteSpace(Sort))
            {
                string[] sorts = Sort.Split(new[] { "," }, StringSplitOptions.None);
#if !NETSTANDARD1_0
                if (sorts.AsParallel().Any(x => string.IsNullOrWhiteSpace(x) || !Regex.IsMatch(x, SortPattern)))
#else
                if (sorts.Any(x => string.IsNullOrWhiteSpace(x) || !Regex.IsMatch(x, SortPattern)))
#endif 
                {
                    yield return new ValidationResult($"<{Sort}> does not match '{SortPattern}'.", new[] { nameof(Sort) });
                }
                else
                {
                    IEnumerable<string> properties = typeof(T).GetProperties()
#if !NETSTANDARD1_0
                        .AsParallel()
#endif
                        .Select(x => x.Name);

                    IEnumerable<string> unknowProperties = sorts.Except(properties)
#if !NETSTANDARD1_0
                        .AsParallel()
#endif
                        .Select(x => $"<{x.Trim()}>");

                    if (unknowProperties.Any())
                    {
                        string unknownPropertiesErrorMsg = unknowProperties.Once()
                            ? $"Unknown {unknowProperties.Single()} property."
                            : $"Unknown {string.Join(", ", unknowProperties.OrderBy(x => x))} properties.";

                        yield return new ValidationResult(unknownPropertiesErrorMsg, new[] { nameof(Sort) });

                    }


                }
                
            }
        }
    }
}

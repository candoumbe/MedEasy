using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.API.Models
{
    public abstract class AbstractSearchRequestModel<T> : IValidatableObject
    {
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
        public string Sort { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Page < 1)
            {
                yield return new ValidationResult($"{nameof(Page)} cannot be lower than 1");
            }

            if (PageSize < 1)
            {
                yield return new ValidationResult($"{nameof(PageSize)} cannot be lower than 1");
            }


            if (!string.IsNullOrWhiteSpace(Sort))
            {
                string[] sorts = Sort.Split(new[] { "," }, StringSplitOptions.None);
            }
        }
    }
}

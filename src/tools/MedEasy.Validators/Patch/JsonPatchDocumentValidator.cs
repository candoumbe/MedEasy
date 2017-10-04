using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Collections.Generic;
using System.Linq;
using static FluentValidation.CascadeMode;

namespace MedEasy.Validators.Patch
{
    /// <summary>
    /// Validator for <see cref="JsonPatchDocument{TModel}"/> instances
    /// </summary>
    /// <typeparam name="TModel">Type of element the <see cref="JsonPatchDocument{TModel}"/> will be applied onto.</typeparam>
    public class JsonPatchDocumentValidator<TModel> : AbstractValidator<JsonPatchDocument<TModel>>
        where TModel : class
    {
        /// <summary>
        /// Builds a new <see cref="JsonPatchDocumentValidator{TModel}"/> instance.
        /// </summary>
        public JsonPatchDocumentValidator()
        {

            CascadeMode = StopOnFirstFailure;

            RuleFor(x => x.Operations)
                .NotNull()
                .Must(operations => operations.AtLeastOnce()).WithMessage("{PropertyName} must have at least one item.")
                .Must(operations =>
                 {
                     IDictionary<string, IEnumerable<Operation<TModel>>> operationGroups = operations
                        .GroupBy(op => op.path)
                        .ToDictionary();

                     return !operationGroups.Any(x => x.Value.Count() > 1);
                 }).WithMessage("Multiple operations on the same path.");
                
        }
    }
}

using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MedEasy.Validators.Patch
{
    /// <summary>
    /// Validator for <see cref="JsonPatchDocument{TModel}"/> instances
    /// </summary>
    /// <typeparam name="TModel">Type of element the <see cref="JsonPatchDocument{TModel}"/> will be applied onto.</typeparam>
    public class JsonPatchDocumentValidator<TModel> : AbstractValidator<JsonPatchDocument<TModel>>
        where TModel : class
    {
        public JsonPatchDocumentValidator()
        {
            RuleFor(x => x.Operations)
                .NotNull()
                .Must(operations => operations.Any()).WithMessage("{PropertyName} must have at least one item.");
                
        }
    }
}

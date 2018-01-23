using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FluentValidation.CascadeMode;
using static FluentValidation.Severity;

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
                .NotEmpty().WithMessage("{PropertyName} must have at least one item.");

            When(
                x => x.Operations.Any(),
                () =>
                {
#if NETSTANDARD2_0
                    
                    RuleFor(x => x.Operations)
                                    .Must(operations => operations.AtLeastOnce(x => x.OperationType == OperationType.Test))
                                    .WithSeverity(Warning)
                                    .WithMessage(@"No ""test"" operation provided."); 
#endif

                    RuleFor(x => x.Operations)
                        .Must(operations =>
                        {
                            IDictionary<string, IEnumerable<Operation<TModel>>> operationGroups = operations
                               .GroupBy(op => op.path)
                               .ToDictionary();

                            return !operationGroups.Any(x => x.Value.AtLeast(2));
                        })
                        .WithMessage((patch) =>
                        {
                            IEnumerable<string> properties = patch.Operations
                               .GroupBy(op => op.path)
                               .ToDictionary()
                               .Where(x => x.Value.AtLeast(2))
                               .Select(x => x.Key)
                               .OrderBy(x => x);

                            return $@"Multiple operations on the same path : {string.Join(", ", properties.Select(x => $@"""{x}"""))}";
                        })
                        .WithSeverity(Warning);
                    ;
                }
            );

        }
    }
}

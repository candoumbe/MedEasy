using FluentValidation;
using MedEasy.DTO;
using System;
using System.Linq;
using static FluentValidation.CascadeMode;
using static Microsoft.AspNetCore.JsonPatch.Operations.OperationType;

namespace MedEasy.Validators.Patch
{
    /// <summary>
    /// Validates <see cref="PatchInfo{Guid, PatientInfo}"/> instances.
    /// </summary>
    public class PatchPatientInfoValidator : AbstractValidator<PatchInfo<Guid, PatientInfo>>
    {
        /// <summary>
        /// Builds a <see cref="PatchPatientInfoValidator"/> instance.
        /// </summary>
        public PatchPatientInfoValidator()
        {

            CascadeMode = StopOnFirstFailure;

            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty);

            RuleFor(x => x.PatchDocument)
                .NotNull();

            When(x => x.Id != Guid.Empty,
                () =>
                {
                    RuleFor(x => x.PatchDocument)
                        .SetValidator(new JsonPatchDocumentValidator<PatientInfo>() { CascadeMode = CascadeMode })
                        // The patch document should not replace or remove the identifier 
                        .Must(patchDocument => !patchDocument.Operations.Any(op => new[] { Replace, Remove }.Contains(op.OperationType) && string.Compare($"/{nameof(PatientInfo.Id)}", op.path, true) == 0))
                            .OverridePropertyName(nameof(PatchInfo<Guid, PatientInfo>.PatchDocument));
                }
            );

        }
    }
}

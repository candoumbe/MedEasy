using FluentValidation;
using MedEasy.CQRS.Core.Commands;
using MedEasy.DTO;
using Patients.DTO;
using System;

namespace Patients.Validators.Patient.Commands
{
    /// <summary>
    /// Validates <see cref="IPatchCommand{Guid, PatientInfo}"/> instance.
    /// </summary>
    public class PatchPatientCommandValidator : AbstractValidator<IPatchCommand<Guid, PatientInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="PatchPatientCommandValidator"/>
        /// </summary>
        /// <param name="patchPatientInfoValidator">validator for data of the command.</param>
        public PatchPatientCommandValidator(IValidator<PatchInfo<Guid, PatientInfo>> patchPatientInfoValidator)
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty);
            RuleFor(x => x.Data)
                .SetValidator(patchPatientInfoValidator);
        }
    }
}

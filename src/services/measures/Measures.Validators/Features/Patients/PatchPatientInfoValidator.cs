using FluentValidation;

using Measures.DTO;

using MedEasy.DAL.Interfaces;

using Microsoft.AspNetCore.JsonPatch;

using System;
using System.Linq;

using static System.StringComparison;

namespace Measures.Validators.Commands.Patients
{
    /// <summary>
    /// Validator for <see cref="JsonPatchDocument{PatientInfo}"/>
    /// </summary>
    public class PatchPatientInfoValidator : AbstractValidator<JsonPatchDocument<PatientInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="PatchPatientInfoValidator"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory">Factory to create <see cref="IUnitOfWork"/>s</param>
        public PatchPatientInfoValidator(IUnitOfWorkFactory unitOfWorkFactory)
        {
            if (unitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            RuleFor(x => x.Operations)
                .NotEmpty()
                .WithMessage("Path document must have at least one operation");

            When(x => x.Operations.Any(),
                () =>
                {
                    RuleFor(x => x.Operations)
                        .Must(ops =>
                            !ops.Any(op => $"/{nameof(PatientInfo.Id)}".Equals(op.path, OrdinalIgnoreCase))
                        );

                    When(x => x.Operations.Any(op => $"/{nameof(PatientInfo.Name)}".Equals(op.path, OrdinalIgnoreCase)),
                        () =>
                        {
                            RuleFor(x => x.Operations)
                                .Must(ops => !ops.Any(op => $"/{nameof(PatientInfo.Name)}".Equals(op.path, OrdinalIgnoreCase)
                                    && op.value != null && op.value.ToString().Length > 100))
                                    .WithMessage($"{nameof(PatientInfo.Name)} must have a length of 100 characters at most.");
                        });
                }
            );
        }
    }
}

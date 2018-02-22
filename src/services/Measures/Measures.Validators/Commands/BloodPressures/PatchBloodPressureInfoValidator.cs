using FluentValidation;
using Measures.DTO;
using MedEasy.DAL.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Linq;
using static System.StringComparison;

namespace Measures.Validators.Commands.BloodPressures
{
    /// <summary>
    /// Validator for <see cref="JsonPatchDocument{BloodPressureInfo}"/>
    /// </summary>
    public class PatchBloodPressureInfoValidator : AbstractValidator<JsonPatchDocument<BloodPressureInfo>>
    {
        
        /// <summary>
        /// Builds a new <see cref="PatchBloodPressureInfoValidator"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory">Factory to create <see cref="IUnitOfWork"/>s</param>
        public PatchBloodPressureInfoValidator(IUnitOfWorkFactory unitOfWorkFactory)
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
                            !ops.Any(op => $"/{nameof(BloodPressureInfo.Id)}".Equals(op.path, OrdinalIgnoreCase))
                        );
                }
            );
        }
    }
}

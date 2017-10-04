using System;
using System.Collections.Generic;
using MedEasy.DTO;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;

namespace MedEasy.Validators.Patient
{
    public class CreatePatientCommandValidator : AbstractValidator<ICreatePatientCommand>
    {
        
        /// <summary>
        /// Builds a new <see cref="CreatePatientCommandValidator"/> instance.
        /// </summary>
        /// <param name="patientInfoValidator">Validator for <see cref="CreatePatientInfo"/> instances.</param>
        public CreatePatientCommandValidator(IValidator<CreatePatientInfo> patientInfoValidator)
        {
            
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty);

            RuleFor(x => x.Data).SetValidator(patientInfoValidator);
        }

    }
}
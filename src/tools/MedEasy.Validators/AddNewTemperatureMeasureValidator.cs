using System;
using MedEasy.Objects;
using FluentValidation;
#if DEBUG
#endif

namespace MedEasy.Validators
{

    public class AddNewTemperatureMeasureCommandValidator : AddNewPhysiologicalMeasureCommandValidator<Temperature>
    {
        /// <summary>
        /// Builds a new <see cref="AddNewTemperatureMeasureCommandValidator"/> instance.
        /// </summary>
        public AddNewTemperatureMeasureCommandValidator()
        {
            RuleFor(x => x.PatientId)
                .NotEqual(Guid.Empty);
        }
    }
}

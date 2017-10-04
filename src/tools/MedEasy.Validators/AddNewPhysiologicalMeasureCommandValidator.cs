using System.Collections.Generic;
using System.Threading.Tasks;
using MedEasy.DTO;
using MedEasy.Objects;
using FluentValidation;

namespace MedEasy.Validators
{
    public abstract class AddNewPhysiologicalMeasureCommandValidator<TPhysiologicalMeasureInfo> : AbstractValidator<CreatePhysiologicalMeasureInfo<TPhysiologicalMeasureInfo>>
        where TPhysiologicalMeasureInfo : PhysiologicalMeasurement
    {

    }
}
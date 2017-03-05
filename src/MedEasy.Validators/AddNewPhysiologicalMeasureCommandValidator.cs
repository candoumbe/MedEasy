using System.Collections.Generic;
using System.Threading.Tasks;
using MedEasy.DTO;
using MedEasy.Objects;

namespace MedEasy.Validators
{
    public abstract class AddNewPhysiologicalMeasureCommandValidator<TPhysiologicalMeasureInfo> : IValidate<CreatePhysiologicalMeasureInfo<TPhysiologicalMeasureInfo>>
        where TPhysiologicalMeasureInfo : PhysiologicalMeasurement
    {
        public abstract IEnumerable<Task<ErrorInfo>> Validate(CreatePhysiologicalMeasureInfo<TPhysiologicalMeasureInfo> element);
    }
}
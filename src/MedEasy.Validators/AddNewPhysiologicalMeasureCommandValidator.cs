using System.Collections.Generic;
using System.Threading.Tasks;
using MedEasy.DTO;

namespace MedEasy.Validators
{
    public abstract class AddNewPhysiologicalMeasureCommandValidator<TPhysiologicalMeasureInfo> : IValidate<TPhysiologicalMeasureInfo>
        where TPhysiologicalMeasureInfo : CreatePhysiologicalMeasureInfo
    {
        public abstract IEnumerable<Task<ErrorInfo>> Validate(TPhysiologicalMeasureInfo element);
    }
}
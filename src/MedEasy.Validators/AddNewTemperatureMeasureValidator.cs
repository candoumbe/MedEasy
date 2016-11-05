using MedEasy.Commands.Patient;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MedEasy.Validators.ErrorLevel;
using MedEasy.DTO;
using System;
#if DEBUG
using System.Diagnostics;
#endif

namespace MedEasy.Validators
{

    public class AddNewTemperatureMeasureCommandValidator : AddNewPhysiologicalMeasureCommandValidator<CreateTemperatureInfo>
    {
        public override IEnumerable<Task<ErrorInfo>> Validate(CreateTemperatureInfo input)
        { 
#if DEBUG
            Debug.Assert(input != null);
#endif
            yield break;
        }
    }
}

using MedEasy.Commands.Patient;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MedEasy.Validators.ErrorLevel;
using MedEasy.DTO;
#if DEBUG
using System.Diagnostics;
#endif

namespace MedEasy.Validators
{
    public class AddNewTemperatureMeasureCommandValidator : IValidate<IAddNewTemperatureMeasureCommand>
    {
        public IEnumerable<Task<ErrorInfo>> Validate(IAddNewTemperatureMeasureCommand cmd)
        {
#if DEBUG
            Debug.Assert(cmd != null); 
#endif
            CreateTemperatureInfo data = cmd.Data;
            if (data.PatientId < 1)
            {
                yield return Task.FromResult(new ErrorInfo(nameof(data.PatientId), "", Error));
            }
        }
    }
}

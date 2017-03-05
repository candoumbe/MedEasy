using MedEasy.Commands;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MedEasy.Validators.ErrorLevel;

namespace MedEasy.Validators
{
    /// <summary>
    /// Validator for <see cref="IPatchCommand{TResourceId, TResource}"/> instances for PATCHing <see cref="PatientInfo"/> resources.
    /// </summary>
    public class ValidatePatchPatientCommand : IValidate<IPatchCommand<Guid, Objects.Patient>>
    {
        public IEnumerable<Task<ErrorInfo>> Validate(IPatchCommand<Guid, Objects.Patient> command)
        {
            if (command.Data.Id == Guid.Empty)
            {
                yield return Task.FromResult(new ErrorInfo(nameof(IPatchInfo<Guid,Objects.Patient>.Id), "Id of the resource to patch not set", Error));
            }

            if (command.Data.PatchDocument == null)
            {
                yield return Task.FromResult(new ErrorInfo(nameof(IPatchInfo<Guid, Objects.Patient>.PatchDocument), $"{nameof(IPatchInfo<int, Objects.Patient>.PatchDocument)}", Error));
            }

            else if (!command.Data.PatchDocument.Operations.Any())
            {
                yield return Task.FromResult(new ErrorInfo(nameof(PatchInfo<Guid, Objects.Patient>.PatchDocument), "No change", Error));
            }
            else
            { 
                if (command.Data.PatchDocument.Operations.Any(x => !string.IsNullOrWhiteSpace(x.path) && $"/{nameof(Objects.Patient.Id)}".Equals(x.path.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    yield return Task.FromResult(new ErrorInfo(nameof(PatchInfo<Guid, Objects.Patient>.PatchDocument), "Change the ID is not allowed", Error));
                }
            }        
            // we select all changes that are related to the same path to see if there are the same operation to apply twice or not
            


        }
    }
}

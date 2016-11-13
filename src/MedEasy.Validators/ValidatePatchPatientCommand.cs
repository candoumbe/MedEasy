using MedEasy.Commands;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MedEasy.Validators.ErrorLevel;
using static System.Text.RegularExpressions.RegexOptions;

namespace MedEasy.Validators
{
    /// <summary>
    /// Validator for <see cref="IPatchCommand{TResourceId}"/> instances for PATCHing <see cref="PatientInfo"/> resources.
    /// </summary>
    public class ValidatePatchPatientCommand : IValidate<IPatchCommand<int>>
    {
        private static Regex validPropertyNameRegex = new Regex(@"^[a-zA-Z]+[a-zA-Z0-9\\_]*$", None, TimeSpan.FromSeconds(2));
        private readonly IEnumerable<string> _patchableProperties;

        

        public IEnumerable<Task<ErrorInfo>> Validate(IPatchCommand<int> command)
        {

            if (command.Data.Id == 0)
            {
                yield return Task.FromResult(new ErrorInfo(nameof(PatchInfo<int>.Id), "Id of the resource to patch not set", Error));
            }

            IEnumerable<ChangeInfo> changes = command.Data.Changes;
            if (changes == null || !changes.Any())
            {
                yield return Task.FromResult(new ErrorInfo(nameof(PatchInfo<int>.Changes), "No change", Error));
            }

            IEnumerable<ChangeInfo> invalidPaths = changes
                .AsParallel()
                .Where(x => !x.Path.StartsWith("/") || x.Path.Count(character => character == '/') > 1
                    || ! validPropertyNameRegex.IsMatch(x.Path.Replace("/", string.Empty).Trim()));

            foreach (ChangeInfo invalidPath in invalidPaths)
            {
                yield return Task.FromResult(new ErrorInfo(nameof(ChangeInfo.Path), $"'{invalidPath.Path}' is not valid", Error));
            }

            // we select all changes that are related to the same path to see if there are the same operation to apply twice or not
            changes = changes.Where(x => !invalidPaths.Contains(x));


            IDictionary<string, IEnumerable<ChangeInfo>> dicoChanges = changes
                .AsParallel()
                .GroupBy(x => x.Path)
                .ToDictionary();

            IEnumerable<string> keysWithMultipleChanges = dicoChanges
                .AsParallel()
                .Where(x => x.Value.Count() > 1) // Todo try to optimize this par
                .Select(x => x.Key.Replace("/", string.Empty).Trim());

            foreach (string item in keysWithMultipleChanges)
            {
                yield return Task.FromResult(new ErrorInfo(item, $"Multiple changes for {item}", Error));
            }

            changes = dicoChanges.Where(x => x.Value.Once()).SelectMany(x => x.Value);


            IDictionary<string, Type> properties = typeof(PatientInfo)
                .GetTypeInfo()
                .GetProperties(BindingFlags.Public & BindingFlags.Instance)
                .Where(x => x.PropertyType.GetTypeInfo().IsPrimitive)
                .ToDictionary(x => x.Name, x => x.PropertyType);

            IEnumerable<string> patchableProperties = properties.Keys;

            IEnumerable<ChangeInfo> pathToUnknownProperties = changes
                .AsParallel()
                .Where(x => x.Path!= null && !patchableProperties.Contains(x.Path.Replace("/", string.Empty))
            );

            foreach (ChangeInfo item in pathToUnknownProperties)
            {
                yield return Task.FromResult(new ErrorInfo(nameof(ChangeInfo.Path), $"Unknown property '{item.Path.Replace("/", string.Empty)}'", Error));
            }



        }
    }
}

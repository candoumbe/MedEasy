using System;
using System.Collections.Generic;
using MedEasy.DTO;
using static MedEasy.Validators.ErrorLevel;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;

namespace MedEasy.Validators.Patient
{
    public class CreatePatientCommandValidator : IValidate<ICreatePatientCommand>
    {
        /// <summary>
        /// Gets the max birthdate the current validator allowed to set
        /// </summary>
        public DateTimeOffset? MaxBirthDateAllowed { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxBirthDateAllowed"></param>
        public CreatePatientCommandValidator(DateTimeOffset? maxBirthDateAllowed = null)
        {
            MaxBirthDateAllowed = maxBirthDateAllowed;
        }


        public IEnumerable<Task<ErrorInfo>> Validate(ICreatePatientCommand command)
        {
            if (command == null)
            {
                yield return Task.FromResult(new ErrorInfo(string.Empty, "command cannot be null", Error));
            } 
            else
            {
                CreatePatientInfo data = command.Data;
                if (data == null)
                {
                    yield return Task.FromResult(new ErrorInfo(nameof(CreatePatientCommand.Data), "no data", Error));
                }
                else
                {
                    if (MaxBirthDateAllowed.HasValue && data.BirthDate.HasValue && data.BirthDate.Value > MaxBirthDateAllowed)
                    {
                        yield return Task.FromResult(new ErrorInfo(nameof(CreatePatientInfo.BirthDate), "element is set in the future", Warning));
                    }

                    if (string.IsNullOrWhiteSpace(data.Firstname))
                    {
                        yield return Task.FromResult(new ErrorInfo(nameof(CreatePatientInfo.Firstname), "Not set", Warning));
                    }

                    if (string.IsNullOrWhiteSpace(data.Lastname))
                    {
                        yield return Task.FromResult(new ErrorInfo(nameof(CreatePatientInfo.Lastname), "Not set", Error));
                    }
                }
            }
        }
    }
}
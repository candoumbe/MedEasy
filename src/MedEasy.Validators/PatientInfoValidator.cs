using System.Collections.Generic;
using MedEasy.DTO;

namespace MedEasy.Validators
{
    public class PatientInfoValidator : IValidate<PatientInfo>
    {
        public IEnumerable<ErrorInfo> Validate(PatientInfo element)
        {
            if (element == null)
            {
                yield return new ErrorInfo(string.Empty, "Patient info cannot be null", ErrorLevel.Error);
            }

            else
            {
                if (string.IsNullOrWhiteSpace(element.Firstname) && string.IsNullOrWhiteSpace(element.Lastname))
                {
                    yield return new ErrorInfo(string.Empty, $"{nameof(element.Firstname)} or {nameof(element.Lastname)} must be set", ErrorLevel.Error);
                }
                else if (string.IsNullOrWhiteSpace(element.Lastname))
                {
                    yield return new ErrorInfo(nameof(PatientInfo.Lastname), $"{nameof(element.Lastname)} must be set", ErrorLevel.Error);
                }
                else
                {
                    yield return new ErrorInfo(nameof(element.Firstname), $"{nameof(element.Firstname)} is not set", ErrorLevel.Warning);
                }
            }
        }
    }
}

using System.Collections.Generic;
using MedEasy.DTO;

namespace MedEasy.Validators
{
    public class DoctorInfoValidator : IValidate<BrowsableDoctorInfo>
    {
        public IEnumerable<ErrorInfo> Validate(BrowsableDoctorInfo element)
        {
            if (element == null)
            {
                yield return new ErrorInfo(string.Empty, "Doctor info cannot be null", ErrorLevel.Error);
            }

            else
            {
                if (string.IsNullOrWhiteSpace(element.Firstname) && string.IsNullOrWhiteSpace(element.Lastname))
                {
                    yield return new ErrorInfo(string.Empty, $"{nameof(element.Firstname)} or {nameof(element.Lastname)} must be set", ErrorLevel.Error);
                }
                else if (string.IsNullOrWhiteSpace(element.Lastname))
                {
                    yield return new ErrorInfo(nameof(BrowsableDoctorInfo.Lastname), $"{nameof(element.Lastname)} must be set", ErrorLevel.Error);
                }
                else
                {
                    yield return new ErrorInfo(nameof(element.Firstname), $"{nameof(element.Firstname)} is not set", ErrorLevel.Warning);
                }
            }
        }
    }
}

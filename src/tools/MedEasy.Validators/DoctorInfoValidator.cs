using System.Collections.Generic;
using MedEasy.DTO;
using System.Threading.Tasks;
using static MedEasy.Validators.ErrorLevel;

namespace MedEasy.Validators
{
    public class DoctorInfoValidator : IValidate<DoctorInfo>
    {
        public IEnumerable<Task<ErrorInfo>> Validate(DoctorInfo element)
        {
            
            if (element == null)
            {
                yield return Task.FromResult(new ErrorInfo(string.Empty, "Doctor info cannot be null", Error));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(element.Firstname) && string.IsNullOrWhiteSpace(element.Lastname))
                {
                    yield return Task.FromResult(new ErrorInfo(string.Empty, $"{nameof(element.Firstname)} or {nameof(element.Lastname)} must be set", Error));
                }
                else if (string.IsNullOrWhiteSpace(element.Lastname))
                {
                    yield return Task.FromResult(new ErrorInfo(nameof(element.Lastname), $"{nameof(element.Lastname)} must be set", Error));
                }
                else
                {
                    yield return Task.FromResult(new ErrorInfo(nameof(element.Firstname), $"{nameof(element.Firstname)} is not set", Warning));
                }
            }
        }
    }
}

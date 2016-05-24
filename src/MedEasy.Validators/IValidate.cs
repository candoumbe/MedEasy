using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.Validators
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public interface IValidate<T>
    {
        IEnumerable<ErrorInfo> Validate(T element);
    }

    public class Validator<T> : IValidate<T>
    {
        public IEnumerable<ErrorInfo> Validate(T element)
        {
            return Enumerable.Empty<ErrorInfo>();
        }
    }
}

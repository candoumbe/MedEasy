using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Validators
{
    /// <summary>
    /// Interface of validators
    /// </summary>
    /// <typeparam name="T">Type to validate</typeparam>
    public interface IValidate<in T>
    {
        /// <summary>
        /// Validates the specified <see cref="element"/>.
        /// This method returns a collection of <see cref="Task"/>
        /// </summary>
        /// <param name="element">The element to validate</param>
        /// <returns><see cref="IEnumerable{T}"/>which holds all informations on the validation result</returns>
        IEnumerable<Task<ErrorInfo>> Validate(T element);
    }

    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.Mobile.Core.Services
{
    /// <summary>
    /// Exception thrown when adding a ViewModel mapping when there's already one.
    /// </summary>
    public class ViewModelAlreadyMappedException : Exception
    {
        /// <summary>
        /// Builds a new <see cref="ViewModelAlreadyMappedException"/> instance.
        /// </summary>
        /// <param name="viewModel">ViewModel type</param>
        public ViewModelAlreadyMappedException(Type viewModel) : base($"<{viewModel}> already mapped")
        {
        }
    }
}

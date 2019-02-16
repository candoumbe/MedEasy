using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.Mobile.Core.ViewModels.NavigationData
{
    /// <summary>
    /// Data that can be passed to a <see cref="LostPasswordViewModel"/> when navigating to it
    /// </summary>
    public class LostPasswordNavigationData : INavigationData
    {
        /// <summary>
        /// Email which will be displayed in the <see cref="LostPasswordViewModel"/>
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Hint to whether or not the <see cref="Email"/> can be updated
        /// </summary>
        public bool CanChangeEmail { get; set; }
    }
}

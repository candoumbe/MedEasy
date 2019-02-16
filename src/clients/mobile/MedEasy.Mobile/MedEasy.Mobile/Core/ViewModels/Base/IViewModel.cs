using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.Mobile.Core.ViewModels.Base
{
    /// <summary>
    /// Marks a class as a View model
    /// </summary>
    public interface IViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Called when the model is created
        /// </summary>
        void Prepare();

        /// <summary>
        /// Called when navigating to this model with data.
        /// No logic should be performed
        /// </summary>
        /// <param name="data"></param>
        void Prepare(object data);
    }
}

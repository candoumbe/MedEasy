using System.Threading.Tasks;

namespace MedEasy.Mobile.Core.ViewModels.Base
{
    public abstract class ViewModelBase : ObservableObject
    {

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}

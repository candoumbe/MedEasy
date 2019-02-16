using Xamarin.Forms;

namespace MedEasy.Mobile.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        public Page MainPage
        {
            get => Application.Current.MainPage;
            set => Application.Current.MainPage = value;
        }
    }
}

using MvvmCross.Forms.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MedEasy.Companion
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : MvxContentPage
	{
		public LoginPage ()
		{
			InitializeComponent ();
		}
	}
}
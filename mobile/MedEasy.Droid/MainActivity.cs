using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace MedEasy.Droid
{
    [Activity(Label = "MedEasy.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            string username = FindViewById<EditText>(Resource.Id.TbxUsername).Text;
            string password = FindViewById<EditText>(Resource.Id.TbxPassword).Text;

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.BtnConnect);

            button.Click += delegate
            {
                button.Text = $"{count} clicks!";
                count++;
            };


        }
    }
}


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MedEasy.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LandingPage : TabbedPage
    {
        public LandingPage ()
        {
            InitializeComponent();
        }
    }
}
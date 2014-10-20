using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using FamilyExpenses.CoreModules;
using FamilyExpenses.ViewModels;

namespace FamilyExpenses
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            try
            {
                InitializeComponent();
                NavigationCacheMode = NavigationCacheMode.Required;
                Loaded += delegate
                {
                    var viewmodel = new MainPageViewModel();
                    viewmodel.Initialize(this);
                    DataContext = viewmodel;
                };
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }
    }
}
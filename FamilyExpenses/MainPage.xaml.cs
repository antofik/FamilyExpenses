using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
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
                var d = new MessageDialog("Error while initializing application\n" + ex, "Fatal error");
                d.ShowAsync();
            }
        }
    }
}
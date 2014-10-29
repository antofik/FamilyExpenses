using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
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
                var viewmodel = new MainPageViewModel();

                InitializeComponent();
                viewmodel.Initialize(this);
                NavigationCacheMode = NavigationCacheMode.Required;
                Loaded += delegate
                {
                    DataContext = viewmodel;
                };
                txtLog.DoubleTapped += delegate
                {
                    ((Frame) Window.Current.Content).Navigate(typeof (LogPage), this);
                    //Frame.Navigate(typeof (LogPage), null, new SlideNavigationTransitionInfo());
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
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using FamilyExpenses.ViewModels;

namespace FamilyExpenses
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            Loaded += delegate
            {
                var viewmodel = new MainPageViewModel();
                viewmodel.Initialize(this);
                DataContext = viewmodel;

                txtCost.Focus(FocusState.Programmatic);

          //      list.SelectionChanged += delegate { viewmodel.SaveCommand.Execute(null); };
            };

            txtCost.GotFocus += delegate { row.Height = new GridLength(300); };
            txtCost.LostFocus += delegate { row.Height = new GridLength(0); };
            pivot.PivotItemLoaded += delegate
            {
                if (pivot.SelectedItem != itemHistory)
                    txtCost.Focus(FocusState.Programmatic);
            };
        }

        private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem == itemHistory)
                UpdateHistory();
            else
            {
                /*var timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
                timer.Tick += delegate
                {
                    timer.Stop();
                    txtCost.Focus(FocusState.Programmatic);
                };
                timer.Start();
                */
            }
        }

        private void UpdateHistory()
        {

        }
    }
}
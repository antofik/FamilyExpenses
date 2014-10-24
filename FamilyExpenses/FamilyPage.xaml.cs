using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using FamilyExpenses.CoreModules;

namespace FamilyExpenses
{
    public sealed partial class FamilyPage
    {
        public FamilyPage()
        {
            InitializeComponent();

            Loaded += delegate
            {
                txtPassword.Text = Core.Storage.FamilyPassword;
            };

            cmdSave.Click += delegate
            {
                IsEnabled = false;
                Core.UpdateFamilyPassword(txtPassword.Text, ok => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
                {
                    IsEnabled = true;
                    if (!ok)
                    {
                        txtPassword.Text = Core.Storage.FamilyPassword;
                    }
                }));
            };
        }
    }
}

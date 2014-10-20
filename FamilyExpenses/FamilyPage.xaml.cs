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
                txtPassword.Text = Core.FamilyPassword;
            };

            cmdSave.Click += delegate
            {
                Core.UpdateFamilyPassword(txtPassword.Text);
            };
        }
    }
}

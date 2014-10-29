using Windows.UI.Xaml;
using FamilyExpenses.CoreModules;

namespace FamilyExpenses
{
    public sealed partial class AdvancedPage
    {
        public AdvancedPage()
        {
            InitializeComponent();
        }

        private void ResetRevision(object sender, RoutedEventArgs e)
        {
            Core.Storage.SetRevision(0);
        }

        private void DeleteData(object sender, RoutedEventArgs e)
        {
            Core.Entries.Clear();
            Core.Storage.Save();
            Core.InvokeRefresh();
        }
    }
}
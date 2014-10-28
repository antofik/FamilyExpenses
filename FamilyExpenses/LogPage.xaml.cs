using Windows.Phone.UI.Input;
using Windows.UI.Xaml.Navigation;
using FamilyExpenses.CoreModules;

namespace FamilyExpenses
{
    public sealed partial class LogPage 
    {
        public LogPage()
        {
            InitializeComponent();
            Load();

            Loaded += delegate
            {
                HardwareButtons.BackPressed += BackPresed;
                txtRevision.Text = Core.Storage.Revision.ToString();
            };
            Unloaded += delegate { HardwareButtons.BackPressed -= BackPresed; };

        }

        private void BackPresed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            Frame.GoBack();    
        }

        private void Load()
        {
            listHistory.ItemsSource = Core.Log.History;
            listState.ItemsSource = Core.Entries;
        }
    }
}

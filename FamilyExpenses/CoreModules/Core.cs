using System.Collections.ObjectModel;
using FamilyExpenses.Models;

namespace FamilyExpenses.CoreModules
{
    public static class Core
    {
        public static readonly StorageImpl Storage = new StorageImpl();

        #region Categories

        private static ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        public static ObservableCollection<Category> Categories { get { return _categories; } }

        #endregion

        #region Entries

        private static ObservableCollection<Entry> _entries = new ObservableCollection<Entry>();
        public static ObservableCollection<Entry> Entries { get { return _entries; } set { _entries = value; } }

        #endregion
    }
}
